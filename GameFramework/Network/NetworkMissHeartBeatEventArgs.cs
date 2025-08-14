//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Network
{
    public sealed class NetworkMissHeartBeatEventArgs : GameFrameworkEventArgs
    {
        public NetworkMissHeartBeatEventArgs()
        {
            NetworkChannel = null;
            MissCount = 0;
        }

        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        public int MissCount
        {
            get;
            private set;
        }

        public static NetworkMissHeartBeatEventArgs Create(INetworkChannel networkChannel, int missCount)
        {
            NetworkMissHeartBeatEventArgs networkMissHeartBeatEventArgs = ReferencePool.Acquire<NetworkMissHeartBeatEventArgs>();
            networkMissHeartBeatEventArgs.NetworkChannel = networkChannel;
            networkMissHeartBeatEventArgs.MissCount = missCount;
            return networkMissHeartBeatEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
            MissCount = 0;
        }
    }
}
