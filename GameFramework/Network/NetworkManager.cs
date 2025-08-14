//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private readonly Dictionary<string, NetworkChannelBase> mNetworkChannels;

        private EventHandler<NetworkConnectedEventArgs> mNetworkConnectedEventHandler;
        private EventHandler<NetworkClosedEventArgs> mNetworkClosedEventHandler;
        private EventHandler<NetworkMissHeartBeatEventArgs> mNetworkMissHeartBeatEventHandler;
        private EventHandler<NetworkErrorEventArgs> mNetworkErrorEventHandler;
        private EventHandler<NetworkCustomErrorEventArgs> mNetworkCustomErrorEventHandler;

        public NetworkManager()
        {
            mNetworkChannels = new Dictionary<string, NetworkChannelBase>(StringComparer.Ordinal);
            mNetworkConnectedEventHandler = null;
            mNetworkClosedEventHandler = null;
            mNetworkMissHeartBeatEventHandler = null;
            mNetworkErrorEventHandler = null;
            mNetworkCustomErrorEventHandler = null;
        }

        public int NetworkChannelCount
        {
            get
            {
                return mNetworkChannels.Count;
            }
        }

        public event EventHandler<NetworkConnectedEventArgs> NetworkConnected
        {
            add
            {
                mNetworkConnectedEventHandler += value;
            }
            remove
            {
                mNetworkConnectedEventHandler -= value;
            }
        }

        public event EventHandler<NetworkClosedEventArgs> NetworkClosed
        {
            add
            {
                mNetworkClosedEventHandler += value;
            }
            remove
            {
                mNetworkClosedEventHandler -= value;
            }
        }

        public event EventHandler<NetworkMissHeartBeatEventArgs> NetworkMissHeartBeat
        {
            add
            {
                mNetworkMissHeartBeatEventHandler += value;
            }
            remove
            {
                mNetworkMissHeartBeatEventHandler -= value;
            }
        }

        public event EventHandler<NetworkErrorEventArgs> NetworkError
        {
            add
            {
                mNetworkErrorEventHandler += value;
            }
            remove
            {
                mNetworkErrorEventHandler -= value;
            }
        }

        public event EventHandler<NetworkCustomErrorEventArgs> NetworkCustomError
        {
            add
            {
                mNetworkCustomErrorEventHandler += value;
            }
            remove
            {
                mNetworkCustomErrorEventHandler -= value;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (KeyValuePair<string, NetworkChannelBase> networkChannel in mNetworkChannels)
            {
                networkChannel.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        internal override void Shutdown()
        {
            foreach (KeyValuePair<string, NetworkChannelBase> networkChannel in mNetworkChannels)
            {
                NetworkChannelBase networkChannelBase = networkChannel.Value;
                networkChannelBase.NetworkChannelConnected -= OnNetworkChannelConnected;
                networkChannelBase.NetworkChannelClosed -= OnNetworkChannelClosed;
                networkChannelBase.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                networkChannelBase.NetworkChannelError -= OnNetworkChannelError;
                networkChannelBase.NetworkChannelCustomError -= OnNetworkChannelCustomError;
                networkChannelBase.Shutdown();
            }

            mNetworkChannels.Clear();
        }

        public bool HasNetworkChannel(string name)
        {
            return mNetworkChannels.ContainsKey(name ?? string.Empty);
        }

        public INetworkChannel GetNetworkChannel(string name)
        {
            NetworkChannelBase networkChannel = null;
            if (mNetworkChannels.TryGetValue(name ?? string.Empty, out networkChannel))
            {
                return networkChannel;
            }

            return null;
        }

        public INetworkChannel[] GetAllNetworkChannels()
        {
            int index = 0;
            INetworkChannel[] results = new INetworkChannel[mNetworkChannels.Count];
            foreach (KeyValuePair<string, NetworkChannelBase> networkChannel in mNetworkChannels)
            {
                results[index++] = networkChannel.Value;
            }

            return results;
        }

        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, NetworkChannelBase> networkChannel in mNetworkChannels)
            {
                results.Add(networkChannel.Value);
            }
        }

        public INetworkChannel CreateNetworkChannel(string name, ServiceType serviceType, INetworkChannelHelper networkChannelHelper)
        {
            if (networkChannelHelper == null)
            {
                throw new GameFrameworkException("Network channel helper is invalid.");
            }

            if (networkChannelHelper.PacketHeaderLength < 0)
            {
                throw new GameFrameworkException("Packet header length is invalid.");
            }

            if (HasNetworkChannel(name))
            {
                throw new GameFrameworkException(Utility.Text.Format("Already exist network channel '{0}'.", name ?? string.Empty));
            }

            NetworkChannelBase networkChannel = null;
            switch (serviceType)
            {
                case ServiceType.Tcp:
                    networkChannel = new TcpNetworkChannel(name, networkChannelHelper);
                    break;

                case ServiceType.TcpWithSyncReceive:
                    networkChannel = new TcpWithSyncReceiveNetworkChannel(name, networkChannelHelper);
                    break;

                default:
                    throw new GameFrameworkException(Utility.Text.Format("Not supported service type '{0}'.", serviceType));
            }

            networkChannel.NetworkChannelConnected += OnNetworkChannelConnected;
            networkChannel.NetworkChannelClosed += OnNetworkChannelClosed;
            networkChannel.NetworkChannelMissHeartBeat += OnNetworkChannelMissHeartBeat;
            networkChannel.NetworkChannelError += OnNetworkChannelError;
            networkChannel.NetworkChannelCustomError += OnNetworkChannelCustomError;
            mNetworkChannels.Add(name, networkChannel);
            return networkChannel;
        }

        public bool DestroyNetworkChannel(string name)
        {
            NetworkChannelBase networkChannel = null;
            if (mNetworkChannels.TryGetValue(name ?? string.Empty, out networkChannel))
            {
                networkChannel.NetworkChannelConnected -= OnNetworkChannelConnected;
                networkChannel.NetworkChannelClosed -= OnNetworkChannelClosed;
                networkChannel.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                networkChannel.NetworkChannelError -= OnNetworkChannelError;
                networkChannel.NetworkChannelCustomError -= OnNetworkChannelCustomError;
                networkChannel.Shutdown();
                return mNetworkChannels.Remove(name);
            }

            return false;
        }

        private void OnNetworkChannelConnected(NetworkChannelBase networkChannel, object userData)
        {
            if (mNetworkConnectedEventHandler != null)
            {
                lock (mNetworkConnectedEventHandler)
                {
                    NetworkConnectedEventArgs networkConnectedEventArgs = NetworkConnectedEventArgs.Create(networkChannel, userData);
                    mNetworkConnectedEventHandler(this, networkConnectedEventArgs);
                    ReferencePool.Release(networkConnectedEventArgs);
                }
            }
        }

        private void OnNetworkChannelClosed(NetworkChannelBase networkChannel)
        {
            if (mNetworkClosedEventHandler != null)
            {
                lock (mNetworkClosedEventHandler)
                {
                    NetworkClosedEventArgs networkClosedEventArgs = NetworkClosedEventArgs.Create(networkChannel);
                    mNetworkClosedEventHandler(this, networkClosedEventArgs);
                    ReferencePool.Release(networkClosedEventArgs);
                }
            }
        }

        private void OnNetworkChannelMissHeartBeat(NetworkChannelBase networkChannel, int missHeartBeatCount)
        {
            if (mNetworkMissHeartBeatEventHandler != null)
            {
                lock (mNetworkMissHeartBeatEventHandler)
                {
                    NetworkMissHeartBeatEventArgs networkMissHeartBeatEventArgs = NetworkMissHeartBeatEventArgs.Create(networkChannel, missHeartBeatCount);
                    mNetworkMissHeartBeatEventHandler(this, networkMissHeartBeatEventArgs);
                    ReferencePool.Release(networkMissHeartBeatEventArgs);
                }
            }
        }

        private void OnNetworkChannelError(NetworkChannelBase networkChannel, NetworkErrorCode errorCode, SocketError socketErrorCode, string errorMessage)
        {
            if (mNetworkErrorEventHandler != null)
            {
                lock (mNetworkErrorEventHandler)
                {
                    NetworkErrorEventArgs networkErrorEventArgs = NetworkErrorEventArgs.Create(networkChannel, errorCode, socketErrorCode, errorMessage);
                    mNetworkErrorEventHandler(this, networkErrorEventArgs);
                    ReferencePool.Release(networkErrorEventArgs);
                }
            }
        }

        private void OnNetworkChannelCustomError(NetworkChannelBase networkChannel, object customErrorData)
        {
            if (mNetworkCustomErrorEventHandler != null)
            {
                lock (mNetworkCustomErrorEventHandler)
                {
                    NetworkCustomErrorEventArgs networkCustomErrorEventArgs = NetworkCustomErrorEventArgs.Create(networkChannel, customErrorData);
                    mNetworkCustomErrorEventHandler(this, networkCustomErrorEventArgs);
                    ReferencePool.Release(networkCustomErrorEventArgs);
                }
            }
        }
    }
}
