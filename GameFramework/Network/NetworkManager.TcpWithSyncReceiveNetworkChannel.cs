//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private sealed class TcpWithSyncReceiveNetworkChannel : NetworkChannelBase
        {
            private readonly AsyncCallback mConnectCallback;
            private readonly AsyncCallback mSendCallback;

            public TcpWithSyncReceiveNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                mConnectCallback = ConnectCallback;
                mSendCallback = SendCallback;
            }

            public override ServiceType ServiceType
            {
                get
                {
                    return ServiceType.TcpWithSyncReceive;
                }
            }

            public override void Connect(IPAddress ipAddress, int port, object userData)
            {
                base.Connect(ipAddress, port, userData);
                mSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (mSocket == null)
                {
                    string errorMessage = "Initialize network channel failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                mNetworkChannelHelper.PrepareForConnecting();
                ConnectAsync(ipAddress, port, userData);
            }

            protected override bool ProcessSend()
            {
                if (base.ProcessSend())
                {
                    SendAsync();
                    return true;
                }

                return false;
            }

            protected override void ProcessReceive()
            {
                base.ProcessReceive();
                while (mSocket.Available > 0)
                {
                    if (!ReceiveSync())
                    {
                        break;
                    }
                }
            }

            private void ConnectAsync(IPAddress ipAddress, int port, object userData)
            {
                try
                {
                    mSocket.BeginConnect(ipAddress, port, mConnectCallback, new ConnectState(mSocket, userData));
                }
                catch (Exception exception)
                {
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void ConnectCallback(IAsyncResult ar)
            {
                ConnectState socketUserData = (ConnectState)ar.AsyncState;
                try
                {
                    socketUserData.Socket.EndConnect(ar);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    mActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
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

                if (NetworkChannelConnected != null)
                {
                    NetworkChannelConnected(this, socketUserData.UserData);
                }

                mActive = true;
            }

            private void SendAsync()
            {
                try
                {
                    mSocket.BeginSend(mSendState.Stream.GetBuffer(), (int)mSendState.Stream.Position, (int)(mSendState.Stream.Length - mSendState.Stream.Position), SocketFlags.None, mSendCallback, mSocket);
                }
                catch (Exception exception)
                {
                    mActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void SendCallback(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState;
                if (!socket.Connected)
                {
                    return;
                }

                int bytesSent = 0;
                try
                {
                    bytesSent = socket.EndSend(ar);
                }
                catch (Exception exception)
                {
                    mActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                mSendState.Stream.Position += bytesSent;
                if (mSendState.Stream.Position < mSendState.Stream.Length)
                {
                    SendAsync();
                    return;
                }

                mSentPacketCount++;
                mSendState.Reset();
            }

            private bool ReceiveSync()
            {
                try
                {
                    int bytesReceived = mSocket.Receive(mReceiveState.Stream.GetBuffer(), (int)mReceiveState.Stream.Position, (int)(mReceiveState.Stream.Length - mReceiveState.Stream.Position), SocketFlags.None);
                    if (bytesReceived <= 0)
                    {
                        Close();
                        return false;
                    }

                    mReceiveState.Stream.Position += bytesReceived;
                    if (mReceiveState.Stream.Position < mReceiveState.Stream.Length)
                    {
                        return false;
                    }

                    mReceiveState.Stream.Position = 0L;

                    bool processSuccess = false;
                    if (mReceiveState.PacketHeader != null)
                    {
                        processSuccess = ProcessPacket();
                        mReceivedPacketCount++;
                    }
                    else
                    {
                        processSuccess = ProcessPacketHeader();
                    }

                    return processSuccess;
                }
                catch (Exception exception)
                {
                    mActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }
            }
        }
    }
}
