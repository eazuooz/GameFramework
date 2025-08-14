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
        private sealed class WebRequestAgent : ITaskAgent<WebRequestTask>
        {
            private readonly IWebRequestAgentHelper mHelper;
            private WebRequestTask mTask;
            private float mWaitTime;

            public GameFrameworkAction<WebRequestAgent> WebRequestAgentStart;
            public GameFrameworkAction<WebRequestAgent, byte[]> WebRequestAgentSuccess;
            public GameFrameworkAction<WebRequestAgent, string> WebRequestAgentFailure;

            public WebRequestAgent(IWebRequestAgentHelper webRequestAgentHelper)
            {
                if (webRequestAgentHelper == null)
                {
                    throw new GameFrameworkException("Web request agent helper is invalid.");
                }

                mHelper = webRequestAgentHelper;
                mTask = null;
                mWaitTime = 0f;

                WebRequestAgentStart = null;
                WebRequestAgentSuccess = null;
                WebRequestAgentFailure = null;
            }

            public WebRequestTask Task
            {
                get
                {
                    return mTask;
                }
            }

            public float WaitTime
            {
                get
                {
                    return mWaitTime;
                }
            }

            public void Initialize()
            {
                mHelper.WebRequestAgentHelperComplete += OnWebRequestAgentHelperComplete;
                mHelper.WebRequestAgentHelperError += OnWebRequestAgentHelperError;
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (mTask.Status == WebRequestTaskStatus.Doing)
                {
                    mWaitTime += realElapseSeconds;
                    if (mWaitTime >= mTask.Timeout)
                    {
                        WebRequestAgentHelperErrorEventArgs webRequestAgentHelperErrorEventArgs = WebRequestAgentHelperErrorEventArgs.Create("Timeout");
                        OnWebRequestAgentHelperError(this, webRequestAgentHelperErrorEventArgs);
                        ReferencePool.Release(webRequestAgentHelperErrorEventArgs);
                    }
                }
            }

            public void Shutdown()
            {
                Reset();
                mHelper.WebRequestAgentHelperComplete -= OnWebRequestAgentHelperComplete;
                mHelper.WebRequestAgentHelperError -= OnWebRequestAgentHelperError;
            }

            public StartTaskStatus Start(WebRequestTask task)
            {
                if (task == null)
                {
                    throw new GameFrameworkException("Task is invalid.");
                }

                mTask = task;
                mTask.Status = WebRequestTaskStatus.Doing;

                if (WebRequestAgentStart != null)
                {
                    WebRequestAgentStart(this);
                }

                byte[] postData = mTask.GetPostData();
                if (postData == null)
                {
                    mHelper.Request(mTask.WebRequestUri, mTask.UserData);
                }
                else
                {
                    mHelper.Request(mTask.WebRequestUri, postData, mTask.UserData);
                }

                mWaitTime = 0f;
                return StartTaskStatus.CanResume;
            }

            public void Reset()
            {
                mHelper.Reset();
                mTask = null;
                mWaitTime = 0f;
            }

            private void OnWebRequestAgentHelperComplete(object sender, WebRequestAgentHelperCompleteEventArgs e)
            {
                mHelper.Reset();
                mTask.Status = WebRequestTaskStatus.Done;

                if (WebRequestAgentSuccess != null)
                {
                    WebRequestAgentSuccess(this, e.GetWebResponseBytes());
                }

                mTask.Done = true;
            }

            private void OnWebRequestAgentHelperError(object sender, WebRequestAgentHelperErrorEventArgs e)
            {
                mHelper.Reset();
                mTask.Status = WebRequestTaskStatus.Error;

                if (WebRequestAgentFailure != null)
                {
                    WebRequestAgentFailure(this, e.ErrorMessage);
                }

                mTask.Done = true;
            }
        }
    }
}
