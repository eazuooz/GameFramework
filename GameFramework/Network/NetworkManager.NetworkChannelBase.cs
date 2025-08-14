//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private abstract class NetworkChannelBase : INetworkChannel, IDisposable
        {
            private const float DefaultHeartBeatInterval = 30f;

            private readonly string mName;
            protected readonly Queue<Packet> mSendPacketPool;
            protected readonly EventPool<Packet> mReceivePacketPool;
            protected readonly INetworkChannelHelper mNetworkChannelHelper;
            protected AddressFamily mAddressFamily;
            protected bool mResetHeartBeatElapseSecondsWhenReceivePacket;
            protected float mHeartBeatInterval;
            protected Socket mSocket;
            protected readonly SendState mSendState;
            protected readonly ReceiveState mReceiveState;
            protected readonly HeartBeatState mHeartBeatState;
            protected int mSentPacketCount;
            protected int mReceivedPacketCount;
            protected bool mActive;
            private bool mDisposed;

            public GameFrameworkAction<NetworkChannelBase, object> NetworkChannelConnected;
            public GameFrameworkAction<NetworkChannelBase> NetworkChannelClosed;
            public GameFrameworkAction<NetworkChannelBase, int> NetworkChannelMissHeartBeat;
            public GameFrameworkAction<NetworkChannelBase, NetworkErrorCode, SocketError, string> NetworkChannelError;
            public GameFrameworkAction<NetworkChannelBase, object> NetworkChannelCustomError;

            public NetworkChannelBase(string name, INetworkChannelHelper networkChannelHelper)
            {
                mName = name ?? string.Empty;
                mSendPacketPool = new Queue<Packet>();
                mReceivePacketPool = new EventPool<Packet>(EventPoolMode.Default);
                mNetworkChannelHelper = networkChannelHelper;
                mAddressFamily = AddressFamily.Unknown;
                mResetHeartBeatElapseSecondsWhenReceivePacket = false;
                mHeartBeatInterval = DefaultHeartBeatInterval;
                mSocket = null;
                mSendState = new SendState();
                mReceiveState = new ReceiveState();
                mHeartBeatState = new HeartBeatState();
                mSentPacketCount = 0;
                mReceivedPacketCount = 0;
                mActive = false;
                mDisposed = false;

                NetworkChannelConnected = null;
                NetworkChannelClosed = null;
                NetworkChannelMissHeartBeat = null;
                NetworkChannelError = null;
                NetworkChannelCustomError = null;

                networkChannelHelper.Initialize(this);
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public Socket Socket
            {
                get
                {
                    return mSocket;
                }
            }

            public bool Connected
            {
                get
                {
                    if (mSocket != null)
                    {
                        return mSocket.Connected;
                    }

                    return false;
                }
            }

            public abstract ServiceType ServiceType
            {
                get;
            }

            public AddressFamily AddressFamily
            {
                get
                {
                    return mAddressFamily;
                }
            }

            public int SendPacketCount
            {
                get
                {
                    return mSendPacketPool.Count;
                }
            }

            public int SentPacketCount
            {
                get
                {
                    return mSentPacketCount;
                }
            }

            public int ReceivePacketCount
            {
                get
                {
                    return mReceivePacketPool.EventCount;
                }
            }

            public int ReceivedPacketCount
            {
                get
                {
                    return mReceivedPacketCount;
                }
            }

            public bool ResetHeartBeatElapseSecondsWhenReceivePacket
            {
                get
                {
                    return mResetHeartBeatElapseSecondsWhenReceivePacket;
                }
                set
                {
                    mResetHeartBeatElapseSecondsWhenReceivePacket = value;
                }
            }

            public int MissHeartBeatCount
            {
                get
                {
                    return mHeartBeatState.MissHeartBeatCount;
                }
            }

            public float HeartBeatInterval
            {
                get
                {
                    return mHeartBeatInterval;
                }
                set
                {
                    mHeartBeatInterval = value;
                }
            }

            public float HeartBeatElapseSeconds
            {
                get
                {
                    return mHeartBeatState.HeartBeatElapseSeconds;
                }
            }

            public virtual void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (mSocket == null || !mActive)
                {
                    return;
                }

                ProcessSend();
                ProcessReceive();
                if (mSocket == null || !mActive)
                {
                    return;
                }

                mReceivePacketPool.Update(elapseSeconds, realElapseSeconds);

                if (mHeartBeatInterval > 0f)
                {
                    bool sendHeartBeat = false;
                    int missHeartBeatCount = 0;
                    lock (mHeartBeatState)
                    {
                        if (mSocket == null || !mActive)
                        {
                            return;
                        }

                        mHeartBeatState.HeartBeatElapseSeconds += realElapseSeconds;
                        if (mHeartBeatState.HeartBeatElapseSeconds >= mHeartBeatInterval)
                        {
                            sendHeartBeat = true;
                            missHeartBeatCount = mHeartBeatState.MissHeartBeatCount;
                            mHeartBeatState.HeartBeatElapseSeconds = 0f;
                            mHeartBeatState.MissHeartBeatCount++;
                        }
                    }

                    if (sendHeartBeat && mNetworkChannelHelper.SendHeartBeat())
                    {
                        if (missHeartBeatCount > 0 && NetworkChannelMissHeartBeat != null)
                        {
                            NetworkChannelMissHeartBeat(this, missHeartBeatCount);
                        }
                    }
                }
            }

            public virtual void Shutdown()
            {
                Close();
                mReceivePacketPool.Shutdown();
                mNetworkChannelHelper.Shutdown();
            }

            public void RegisterHandler(IPacketHandler handler)
            {
                if (handler == null)
                {
                    throw new GameFrameworkException("Packet handler is invalid.");
                }

                mReceivePacketPool.Subscribe(handler.Id, handler.Handle);
            }

            public void SetDefaultHandler(EventHandler<Packet> handler)
            {
                mReceivePacketPool.SetDefaultHandler(handler);
            }

            public void Connect(IPAddress ipAddress, int port)
            {
                Connect(ipAddress, port, null);
            }

            public virtual void Connect(IPAddress ipAddress, int port, object userData)
            {
                if (mSocket != null)
                {
                    Close();
                    mSocket = null;
                }

                switch (ipAddress.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        mAddressFamily = AddressFamily.IPv4;
                        break;

                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        mAddressFamily = AddressFamily.IPv6;
                        break;

                    default:
                        string errorMessage = Utility.Text.Format("Not supported address family '{0}'.", ipAddress.AddressFamily);
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.AddressFamilyError, SocketError.Success, errorMessage);
                            return;
                        }

                        throw new GameFrameworkException(errorMessage);
                }

                mSendState.Reset();
                mReceiveState.PrepareForPacketHeader(mNetworkChannelHelper.PacketHeaderLength);
            }

            public void Close()
            {
                lock (this)
                {
                    if (mSocket == null)
                    {
                        return;
                    }

                    mActive = false;

                    try
                    {
                        mSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        mSocket.Close();
                        mSocket = null;

                        if (NetworkChannelClosed != null)
                        {
                            NetworkChannelClosed(this);
                        }
                    }

                    mSentPacketCount = 0;
                    mReceivedPacketCount = 0;

                    lock (mSendPacketPool)
                    {
                        mSendPacketPool.Clear();
                    }

                    mReceivePacketPool.Clear();

                    lock (mHeartBeatState)
                    {
                        mHeartBeatState.Reset(true);
                    }
                }
            }

            public void Send<T>(T packet) where T : Packet
            {
                if (mSocket == null)
                {
                    string errorMessage = "You must connect first.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (!mActive)
                {
                    string errorMessage = "Socket is not active.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (packet == null)
                {
                    string errorMessage = "Packet is invalid.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                lock (mSendPacketPool)
                {
                    mSendPacketPool.Enqueue(packet);
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (mDisposed)
                {
                    return;
                }

                if (disposing)
                {
                    Close();
                    mSendState.Dispose();
                    mReceiveState.Dispose();
                }

                mDisposed = true;
            }

            protected virtual bool ProcessSend()
            {
                if (mSendState.Stream.Length > 0 || mSendPacketPool.Count <= 0)
                {
                    return false;
                }

                while (mSendPacketPool.Count > 0)
                {
                    Packet packet = null;
                    lock (mSendPacketPool)
                    {
                        packet = mSendPacketPool.Dequeue();
                    }

                    bool serializeResult = false;
                    try
                    {
                        serializeResult = mNetworkChannelHelper.Serialize(packet, mSendState.Stream);
                    }
                    catch (Exception exception)
                    {
                        mActive = false;
                        if (NetworkChannelError != null)
                        {
                            SocketException socketException = exception as SocketException;
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                            return false;
                        }

                        throw;
                    }

                    if (!serializeResult)
                    {
                        string errorMessage = "Serialized packet failure.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }
                }

                mSendState.Stream.Position = 0L;
                return true;
            }

            protected virtual void ProcessReceive()
            {
            }

            protected virtual bool ProcessPacketHeader()
            {
                try
                {
                    object customErrorData = null;
                    IPacketHeader packetHeader = mNetworkChannelHelper.DeserializePacketHeader(mReceiveState.Stream, out customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packetHeader == null)
                    {
                        string errorMessage = "Packet header is invalid.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }

                    mReceiveState.PrepareForPacket(packetHeader);
                    if (packetHeader.PacketLength <= 0)
                    {
                        bool processSuccess = ProcessPacket();
                        mReceivedPacketCount++;
                        return processSuccess;
                    }
                }
                catch (Exception exception)
                {
                    mActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }

            protected virtual bool ProcessPacket()
            {
                lock (mHeartBeatState)
                {
                    mHeartBeatState.Reset(mResetHeartBeatElapseSecondsWhenReceivePacket);
                }

                try
                {
                    object customErrorData = null;
                    Packet packet = mNetworkChannelHelper.DeserializePacket(mReceiveState.PacketHeader, mReceiveState.Stream, out customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packet != null)
                    {
                        mReceivePacketPool.Fire(this, packet);
                    }

                    mReceiveState.PrepareForPacketHeader(mNetworkChannelHelper.PacketHeaderLength);
                }
                catch (Exception exception)
                {
                    mActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }
        }
    }
}
