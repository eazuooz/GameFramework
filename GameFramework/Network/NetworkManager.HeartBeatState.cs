//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private sealed class HeartBeatState
        {
            private float mHeartBeatElapseSeconds;
            private int mMissHeartBeatCount;

            public HeartBeatState()
            {
                mHeartBeatElapseSeconds = 0f;
                mMissHeartBeatCount = 0;
            }

            public float HeartBeatElapseSeconds
            {
                get
                {
                    return mHeartBeatElapseSeconds;
                }
                set
                {
                    mHeartBeatElapseSeconds = value;
                }
            }

            public int MissHeartBeatCount
            {
                get
                {
                    return mMissHeartBeatCount;
                }
                set
                {
                    mMissHeartBeatCount = value;
                }
            }

            public void Reset(bool resetHeartBeatElapseSeconds)
            {
                if (resetHeartBeatElapseSeconds)
                {
                    mHeartBeatElapseSeconds = 0f;
                }

                mMissHeartBeatCount = 0;
            }
        }
    }
}
