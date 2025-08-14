//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.WebRequest
{
    public interface IWebRequestAgentHelper
    {
        event EventHandler<WebRequestAgentHelperCompleteEventArgs> WebRequestAgentHelperComplete;

        event EventHandler<WebRequestAgentHelperErrorEventArgs> WebRequestAgentHelperError;

        void Request(string webRequestUri, object userData);

        void Request(string webRequestUri, byte[] postData, object userData);

        void Reset();
    }
}
