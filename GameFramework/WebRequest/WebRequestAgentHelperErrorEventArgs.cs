//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.WebRequest
{
    public sealed class WebRequestAgentHelperErrorEventArgs : GameFrameworkEventArgs
    {
        public WebRequestAgentHelperErrorEventArgs()
        {
            ErrorMessage = null;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public static WebRequestAgentHelperErrorEventArgs Create(string errorMessage)
        {
            WebRequestAgentHelperErrorEventArgs webRequestAgentHelperErrorEventArgs = ReferencePool.Acquire<WebRequestAgentHelperErrorEventArgs>();
            webRequestAgentHelperErrorEventArgs.ErrorMessage = errorMessage;
            return webRequestAgentHelperErrorEventArgs;
        }

        public override void Clear()
        {
            ErrorMessage = null;
        }
    }
}
