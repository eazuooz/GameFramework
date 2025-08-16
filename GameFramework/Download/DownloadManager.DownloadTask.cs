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
        private sealed class DownloadTask : TaskBase
        {
            private static int Serial = 0;

            private DownloadTaskStatus mStatus;
            private string mDownloadPath;
            private string mDownloadUri;
            private int mFlushSize;
            private float mTimeout;

            public DownloadTask()
            {
                mStatus = DownloadTaskStatus.Todo;
                mDownloadPath = null;
                mDownloadUri = null;
                mFlushSize = 0;
                mTimeout = 0f;
            }

            public DownloadTaskStatus Status
            {
                get
                {
                    return mStatus;
                }
                set
                {
                    mStatus = value;
                }
            }

            public string DownloadPath
            {
                get
                {
                    return mDownloadPath;
                }
            }

            public string DownloadUri
            {
                get
                {
                    return mDownloadUri;
                }
            }

            public int FlushSize
            {
                get
                {
                    return mFlushSize;
                }
            }

            public float Timeout
            {
                get
                {
                    return mTimeout;
                }
            }

            public override string Description
            {
                get
                {
                    return mDownloadPath;
                }
            }

            public static DownloadTask Create(string downloadPath, string downloadUri, string tag, int priority, int flushSize, float timeout, object userData)
            {
                DownloadTask downloadTask = ReferencePool.Acquire<DownloadTask>();
                downloadTask.Initialize(++Serial, tag, priority, userData);
                downloadTask.mDownloadPath = downloadPath;
                downloadTask.mDownloadUri = downloadUri;
                downloadTask.mFlushSize = flushSize;
                downloadTask.mTimeout = timeout;
                return downloadTask;
            }

            public override void Clear()
            {
                base.Clear();
                mStatus = DownloadTaskStatus.Todo;
                mDownloadPath = null;
                mDownloadUri = null;
                mFlushSize = 0;
                mTimeout = 0f;
            }
        }
    }
}
