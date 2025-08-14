//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.WebRequest
{
    internal sealed partial class WebRequestManager : GameFrameworkModule, IWebRequestManager
    {
        private sealed class WebRequestTask : TaskBase
        {
            private static int s_Serial = 0;

            private WebRequestTaskStatus mStatus;
            private string mWebRequestUri;
            private byte[] mPostData;
            private float mTimeout;

            public WebRequestTask()
            {
                mStatus = WebRequestTaskStatus.Todo;
                mWebRequestUri = null;
                mPostData = null;
                mTimeout = 0f;
            }

            public WebRequestTaskStatus Status
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

            public string WebRequestUri
            {
                get
                {
                    return mWebRequestUri;
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
                    return mWebRequestUri;
                }
            }

            public static WebRequestTask Create(string webRequestUri, byte[] postData, string tag, int priority, float timeout, object userData)
            {
                WebRequestTask webRequestTask = ReferencePool.Acquire<WebRequestTask>();
                webRequestTask.Initialize(++s_Serial, tag, priority, userData);
                webRequestTask.mWebRequestUri = webRequestUri;
                webRequestTask.mPostData = postData;
                webRequestTask.mTimeout = timeout;
                return webRequestTask;
            }

            public override void Clear()
            {
                base.Clear();
                mStatus = WebRequestTaskStatus.Todo;
                mWebRequestUri = null;
                mPostData = null;
                mTimeout = 0f;
            }

            public byte[] GetPostData()
            {
                return mPostData;
            }
        }
    }
}
