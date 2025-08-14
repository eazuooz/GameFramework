//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Download
{
    internal sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        private sealed partial class DownloadCounter
        {
            private sealed class DownloadCounterNode : IReference
            {
                private long mDeltaLength;
                private float mElapseSeconds;

                public DownloadCounterNode()
                {
                    mDeltaLength = 0L;
                    mElapseSeconds = 0f;
                }

                public long DeltaLength
                {
                    get
                    {
                        return mDeltaLength;
                    }
                }

                public float ElapseSeconds
                {
                    get
                    {
                        return mElapseSeconds;
                    }
                }

                public static DownloadCounterNode Create()
                {
                    return ReferencePool.Acquire<DownloadCounterNode>();
                }

                public void Update(float elapseSeconds, float realElapseSeconds)
                {
                    mElapseSeconds += realElapseSeconds;
                }

                public void AddDeltaLength(int deltaLength)
                {
                    mDeltaLength += deltaLength;
                }

                public void Clear()
                {
                    mDeltaLength = 0L;
                    mElapseSeconds = 0f;
                }
            }
        }
    }
}
