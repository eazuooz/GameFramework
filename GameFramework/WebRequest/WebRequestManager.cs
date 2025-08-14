//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework.WebRequest
{
    internal sealed partial class WebRequestManager : GameFrameworkModule, IWebRequestManager
    {
        private readonly TaskPool<WebRequestTask> mTaskPool;
        private float mTimeout;
        private EventHandler<WebRequestStartEventArgs> mWebRequestStartEventHandler;
        private EventHandler<WebRequestSuccessEventArgs> mWebRequestSuccessEventHandler;
        private EventHandler<WebRequestFailureEventArgs> mWebRequestFailureEventHandler;

        public WebRequestManager()
        {
            mTaskPool = new TaskPool<WebRequestTask>();
            mTimeout = 30f;
            mWebRequestStartEventHandler = null;
            mWebRequestSuccessEventHandler = null;
            mWebRequestFailureEventHandler = null;
        }

        public int TotalAgentCount
        {
            get
            {
                return mTaskPool.TotalAgentCount;
            }
        }

        public int FreeAgentCount
        {
            get
            {
                return mTaskPool.FreeAgentCount;
            }
        }

        public int WorkingAgentCount
        {
            get
            {
                return mTaskPool.WorkingAgentCount;
            }
        }

        public int WaitingTaskCount
        {
            get
            {
                return mTaskPool.WaitingTaskCount;
            }
        }

        public float Timeout
        {
            get
            {
                return mTimeout;
            }
            set
            {
                mTimeout = value;
            }
        }

        public event EventHandler<WebRequestStartEventArgs> WebRequestStart
        {
            add
            {
                mWebRequestStartEventHandler += value;
            }
            remove
            {
                mWebRequestStartEventHandler -= value;
            }
        }

        public event EventHandler<WebRequestSuccessEventArgs> WebRequestSuccess
        {
            add
            {
                mWebRequestSuccessEventHandler += value;
            }
            remove
            {
                mWebRequestSuccessEventHandler -= value;
            }
        }

        public event EventHandler<WebRequestFailureEventArgs> WebRequestFailure
        {
            add
            {
                mWebRequestFailureEventHandler += value;
            }
            remove
            {
                mWebRequestFailureEventHandler -= value;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            mTaskPool.Update(elapseSeconds, realElapseSeconds);
        }

        internal override void Shutdown()
        {
            mTaskPool.Shutdown();
        }

        public void AddWebRequestAgentHelper(IWebRequestAgentHelper webRequestAgentHelper)
        {
            WebRequestAgent agent = new WebRequestAgent(webRequestAgentHelper);
            agent.WebRequestAgentStart += OnWebRequestAgentStart;
            agent.WebRequestAgentSuccess += OnWebRequestAgentSuccess;
            agent.WebRequestAgentFailure += OnWebRequestAgentFailure;

            mTaskPool.AddAgent(agent);
        }

        public TaskInfo GetWebRequestInfo(int serialId)
        {
            return mTaskPool.GetTaskInfo(serialId);
        }

        public TaskInfo[] GetWebRequestInfos(string tag)
        {
            return mTaskPool.GetTaskInfos(tag);
        }

        public void GetAllWebRequestInfos(string tag, List<TaskInfo> results)
        {
            mTaskPool.GetTaskInfos(tag, results);
        }

        public TaskInfo[] GetAllWebRequestInfos()
        {
            return mTaskPool.GetAllTaskInfos();
        }

        public void GetAllWebRequestInfos(List<TaskInfo> results)
        {
            mTaskPool.GetAllTaskInfos(results);
        }

        public int AddWebRequest(string webRequestUri)
        {
            return AddWebRequest(webRequestUri, null, null, Constant.DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData)
        {
            return AddWebRequest(webRequestUri, postData, null, Constant.DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, string tag)
        {
            return AddWebRequest(webRequestUri, null, tag, Constant.DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, int priority)
        {
            return AddWebRequest(webRequestUri, null, null, priority, null);
        }

        public int AddWebRequest(string webRequestUri, object userData)
        {
            return AddWebRequest(webRequestUri, null, null, Constant.DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, string tag)
        {
            return AddWebRequest(webRequestUri, postData, tag, Constant.DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, int priority)
        {
            return AddWebRequest(webRequestUri, postData, null, priority, null);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, object userData)
        {
            return AddWebRequest(webRequestUri, postData, null, Constant.DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, string tag, int priority)
        {
            return AddWebRequest(webRequestUri, null, tag, priority, null);
        }

        public int AddWebRequest(string webRequestUri, string tag, object userData)
        {
            return AddWebRequest(webRequestUri, null, tag, Constant.DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, null, null, priority, userData);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, string tag, int priority)
        {
            return AddWebRequest(webRequestUri, postData, tag, priority, null);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, string tag, object userData)
        {
            return AddWebRequest(webRequestUri, postData, tag, Constant.DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, postData, null, priority, userData);
        }

        public int AddWebRequest(string webRequestUri, string tag, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, null, tag, priority, userData);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, string tag, int priority, object userData)
        {
            if (string.IsNullOrEmpty(webRequestUri))
            {
                throw new GameFrameworkException("Web request uri is invalid.");
            }

            if (TotalAgentCount <= 0)
            {
                throw new GameFrameworkException("You must add web request agent first.");
            }

            WebRequestTask webRequestTask = WebRequestTask.Create(webRequestUri, postData, tag, priority, mTimeout, userData);
            mTaskPool.AddTask(webRequestTask);
            return webRequestTask.SerialId;
        }

        public bool RemoveWebRequest(int serialId)
        {
            return mTaskPool.RemoveTask(serialId);
        }

        public int RemoveWebRequests(string tag)
        {
            return mTaskPool.RemoveTasks(tag);
        }

        public int RemoveAllWebRequests()
        {
            return mTaskPool.RemoveAllTasks();
        }

        private void OnWebRequestAgentStart(WebRequestAgent sender)
        {
            if (mWebRequestStartEventHandler != null)
            {
                WebRequestStartEventArgs webRequestStartEventArgs = WebRequestStartEventArgs.Create(sender.Task.SerialId, sender.Task.WebRequestUri, sender.Task.UserData);
                mWebRequestStartEventHandler(this, webRequestStartEventArgs);
                ReferencePool.Release(webRequestStartEventArgs);
            }
        }

        private void OnWebRequestAgentSuccess(WebRequestAgent sender, byte[] webResponseBytes)
        {
            if (mWebRequestSuccessEventHandler != null)
            {
                WebRequestSuccessEventArgs webRequestSuccessEventArgs = WebRequestSuccessEventArgs.Create(sender.Task.SerialId, sender.Task.WebRequestUri, webResponseBytes, sender.Task.UserData);
                mWebRequestSuccessEventHandler(this, webRequestSuccessEventArgs);
                ReferencePool.Release(webRequestSuccessEventArgs);
            }
        }

        private void OnWebRequestAgentFailure(WebRequestAgent sender, string errorMessage)
        {
            if (mWebRequestFailureEventHandler != null)
            {
                WebRequestFailureEventArgs webRequestFailureEventArgs = WebRequestFailureEventArgs.Create(sender.Task.SerialId, sender.Task.WebRequestUri, errorMessage, sender.Task.UserData);
                mWebRequestFailureEventHandler(this, webRequestFailureEventArgs);
                ReferencePool.Release(webRequestFailureEventArgs);
            }
        }
    }
}
