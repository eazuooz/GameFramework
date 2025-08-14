//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.WebRequest
{
    public sealed class WebRequestSuccessEventArgs : GameFrameworkEventArgs
    {
        private byte[] mWebResponseBytes;

        public WebRequestSuccessEventArgs()
        {
            SerialId = 0;
            WebRequestUri = null;
            mWebResponseBytes = null;
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

        public static WebRequestSuccessEventArgs Create(int serialId, string webRequestUri, byte[] webResponseBytes, object userData)
        {
            WebRequestSuccessEventArgs webRequestSuccessEventArgs = ReferencePool.Acquire<WebRequestSuccessEventArgs>();
            webRequestSuccessEventArgs.SerialId = serialId;
            webRequestSuccessEventArgs.WebRequestUri = webRequestUri;
            webRequestSuccessEventArgs.mWebResponseBytes = webResponseBytes;
            webRequestSuccessEventArgs.UserData = userData;
            return webRequestSuccessEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            WebRequestUri = null;
            mWebResponseBytes = null;
            UserData = null;
        }

        public byte[] GetWebResponseBytes()
        {
            return mWebResponseBytes;
        }
    }
}
