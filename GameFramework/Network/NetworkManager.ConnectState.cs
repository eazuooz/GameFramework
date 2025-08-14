//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Net.Sockets;

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private sealed class ConnectState
        {
            private readonly Socket mSocket;
            private readonly object mUserData;

            public ConnectState(Socket socket, object userData)
            {
                mSocket = socket;
                mUserData = userData;
            }

            public Socket Socket
            {
                get
                {
                    return mSocket;
                }
            }

            public object UserData
            {
                get
                {
                    return mUserData;
                }
            }
        }
    }
}
