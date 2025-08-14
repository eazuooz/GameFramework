//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.WebRequest
{
    public sealed class WebRequestFailureEventArgs : GameFrameworkEventArgs
    {
        public WebRequestFailureEventArgs()
        {
            SerialId = 0;
            WebRequestUri = null;
            ErrorMessage = null;
            UserData = null;
        }

        public int SerialId
        {
            get;
            private set;
        }

        public string WebRequestUri
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static WebRequestFailureEventArgs Create(int serialId, string webRequestUri, string errorMessage, object userData)
        {
            WebRequestFailureEventArgs webRequestFailureEventArgs = ReferencePool.Acquire<WebRequestFailureEventArgs>();
            webRequestFailureEventArgs.SerialId = serialId;
            webRequestFailureEventArgs.WebRequestUri = webRequestUri;
            webRequestFailureEventArgs.ErrorMessage = errorMessage;
            webRequestFailureEventArgs.UserData = userData;
            return webRequestFailureEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            WebRequestUri = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
