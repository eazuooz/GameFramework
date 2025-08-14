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
    public interface IWebRequestManager
    {
        int TotalAgentCount
        {
            get;
        }

        int FreeAgentCount
        {
            get;
        }

        int WorkingAgentCount
        {
            get;
        }

        int WaitingTaskCount
        {
            get;
        }

        float Timeout
        {
            get;
            set;
        }

        event EventHandler<WebRequestStartEventArgs> WebRequestStart;

        event EventHandler<WebRequestSuccessEventArgs> WebRequestSuccess;

        event EventHandler<WebRequestFailureEventArgs> WebRequestFailure;

        void AddWebRequestAgentHelper(IWebRequestAgentHelper webRequestAgentHelper);

        TaskInfo GetWebRequestInfo(int serialId);

        TaskInfo[] GetWebRequestInfos(string tag);

        void GetAllWebRequestInfos(string tag, List<TaskInfo> results);

        TaskInfo[] GetAllWebRequestInfos();

        void GetAllWebRequestInfos(List<TaskInfo> results);

        int AddWebRequest(string webRequestUri);

        int AddWebRequest(string webRequestUri, byte[] postData);

        int AddWebRequest(string webRequestUri, string tag);

        int AddWebRequest(string webRequestUri, int priority);

        int AddWebRequest(string webRequestUri, object userData);

        int AddWebRequest(string webRequestUri, byte[] postData, string tag);

        int AddWebRequest(string webRequestUri, byte[] postData, int priority);

        int AddWebRequest(string webRequestUri, byte[] postData, object userData);

        int AddWebRequest(string webRequestUri, string tag, int priority);

        int AddWebRequest(string webRequestUri, string tag, object userData);

        int AddWebRequest(string webRequestUri, int priority, object userData);

        int AddWebRequest(string webRequestUri, byte[] postData, string tag, int priority);

        int AddWebRequest(string webRequestUri, byte[] postData, string tag, object userData);

        int AddWebRequest(string webRequestUri, byte[] postData, int priority, object userData);

        int AddWebRequest(string webRequestUri, string tag, int priority, object userData);

        int AddWebRequest(string webRequestUri, byte[] postData, string tag, int priority, object userData);

        bool RemoveWebRequest(int serialId);

        int RemoveWebRequests(string tag);

        int RemoveAllWebRequests();
    }
}
