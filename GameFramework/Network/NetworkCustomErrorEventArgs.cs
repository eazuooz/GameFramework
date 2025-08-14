//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Network
{
    public sealed class NetworkCustomErrorEventArgs : GameFrameworkEventArgs
    {
        public NetworkCustomErrorEventArgs()
        {
            NetworkChannel = null;
            CustomErrorData = null;
        }

        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        public object CustomErrorData
        {
            get;
            private set;
        }

        public static NetworkCustomErrorEventArgs Create(INetworkChannel networkChannel, object customErrorData)
        {
            NetworkCustomErrorEventArgs networkCustomErrorEventArgs = ReferencePool.Acquire<NetworkCustomErrorEventArgs>();
            networkCustomErrorEventArgs.NetworkChannel = networkChannel;
            networkCustomErrorEventArgs.CustomErrorData = customErrorData;
            return networkCustomErrorEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
            CustomErrorData = null;
        }
    }
}
