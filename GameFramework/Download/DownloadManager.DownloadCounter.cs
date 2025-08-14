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
            private readonly GameFrameworkLinkedList<DownloadCounterNode> mDownloadCounterNodes;
            private float mUpdateInterval;
            private float mRecordInterval;
            private float mCurrentSpeed;
            private float mAccumulator;
            private float mTimeLeft;

            public DownloadCounter(float updateInterval, float recordInterval)
            {
                if (updateInterval <= 0f)
                {
                    throw new GameFrameworkException("Update interval is invalid.");
                }

                if (recordInterval <= 0f)
                {
                    throw new GameFrameworkException("Record interval is invalid.");
                }

                mDownloadCounterNodes = new GameFrameworkLinkedList<DownloadCounterNode>();
                mUpdateInterval = updateInterval;
                mRecordInterval = recordInterval;
                Reset();
            }

            public float UpdateInterval
            {
                get
                {
                    return mUpdateInterval;
                }
                set
                {
                    if (value <= 0f)
                    {
                        throw new GameFrameworkException("Update interval is invalid.");
                    }

                    mUpdateInterval = value;
                    Reset();
                }
            }

            public float RecordInterval
            {
                get
                {
                    return mRecordInterval;
                }
                set
                {
                    if (value <= 0f)
                    {
                        throw new GameFrameworkException("Record interval is invalid.");
                    }

                    mRecordInterval = value;
                    Reset();
                }
            }

            public float CurrentSpeed
            {
                get
                {
                    return mCurrentSpeed;
                }
            }

            public void Shutdown()
            {
                Reset();
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (mDownloadCounterNodes.Count <= 0)
                {
                    return;
                }

                mAccumulator += realElapseSeconds;
                if (mAccumulator > mRecordInterval)
                {
                    mAccumulator = mRecordInterval;
                }

                mTimeLeft -= realElapseSeconds;
                foreach (DownloadCounterNode downloadCounterNode in mDownloadCounterNodes)
                {
                    downloadCounterNode.Update(elapseSeconds, realElapseSeconds);
                }

                while (mDownloadCounterNodes.Count > 0)
                {
                    DownloadCounterNode downloadCounterNode = mDownloadCounterNodes.First.Value;
                    if (downloadCounterNode.ElapseSeconds < mRecordInterval)
                    {
                        break;
                    }

                    ReferencePool.Release(downloadCounterNode);
                    mDownloadCounterNodes.RemoveFirst();
                }

                if (mDownloadCounterNodes.Count <= 0)
                {
                    Reset();
                    return;
                }

                if (mTimeLeft <= 0f)
                {
                    long totalDeltaLength = 0L;
                    foreach (DownloadCounterNode downloadCounterNode in mDownloadCounterNodes)
                    {
                        totalDeltaLength += downloadCounterNode.DeltaLength;
                    }

                    mCurrentSpeed = mAccumulator > 0f ? totalDeltaLength / mAccumulator : 0f;
                    mTimeLeft += mUpdateInterval;
                }
            }

            public void RecordDeltaLength(int deltaLength)
            {
                if (deltaLength <= 0)
                {
                    return;
                }

                DownloadCounterNode downloadCounterNode = null;
                if (mDownloadCounterNodes.Count > 0)
                {
                    downloadCounterNode = mDownloadCounterNodes.Last.Value;
                    if (downloadCounterNode.ElapseSeconds < mUpdateInterval)
                    {
                        downloadCounterNode.AddDeltaLength(deltaLength);
                        return;
                    }
                }

                downloadCounterNode = DownloadCounterNode.Create();
                downloadCounterNode.AddDeltaLength(deltaLength);
                mDownloadCounterNodes.AddLast(downloadCounterNode);
            }

            private void Reset()
            {
                mDownloadCounterNodes.Clear();
                mCurrentSpeed = 0f;
                mAccumulator = 0f;
                mTimeLeft = 0f;
            }
        }
    }
}
