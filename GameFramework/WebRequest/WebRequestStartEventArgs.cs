//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.WebRequest
{
    public sealed class WebRequestStartEventArgs : GameFrameworkEventArgs
    {
        public WebRequestStartEventArgs()
        {
            SerialId = 0;
            WebRequestUri = null;
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

        public object UserData
        {
            get;
            private set;
        }

        public static WebRequestStartEventArgs Create(int serialId, string webRequestUri, object userData)
        {
            WebRequestStartEventArgs webRequestStartEventArgs = ReferencePool.Acquire<WebRequestStartEventArgs>();
            webRequestStartEventArgs.SerialId = serialId;
            webRequestStartEventArgs.WebRequestUri = webRequestUri;
            webRequestStartEventArgs.UserData = userData;
            return webRequestStartEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            WebRequestUri = null;
            UserData = null;
        }
    }
}
