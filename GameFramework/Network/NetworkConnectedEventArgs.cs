//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Network
{
    public sealed class NetworkConnectedEventArgs : GameFrameworkEventArgs
    {
        public NetworkConnectedEventArgs()
        {
            NetworkChannel = null;
            UserData = null;
        }

        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static NetworkConnectedEventArgs Create(INetworkChannel networkChannel, object userData)
        {
            NetworkConnectedEventArgs networkConnectedEventArgs = ReferencePool.Acquire<NetworkConnectedEventArgs>();
            networkConnectedEventArgs.NetworkChannel = networkChannel;
            networkConnectedEventArgs.UserData = userData;
            return networkConnectedEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
            UserData = null;
        }
    }
}
