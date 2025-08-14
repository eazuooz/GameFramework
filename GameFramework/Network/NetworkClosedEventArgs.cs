//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Network
{
    public sealed class NetworkClosedEventArgs : GameFrameworkEventArgs
    {
        public NetworkClosedEventArgs()
        {
            NetworkChannel = null;
        }

        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        public static NetworkClosedEventArgs Create(INetworkChannel networkChannel)
        {
            NetworkClosedEventArgs networkClosedEventArgs = ReferencePool.Acquire<NetworkClosedEventArgs>();
            networkClosedEventArgs.NetworkChannel = networkChannel;
            return networkClosedEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
        }
    }
}
