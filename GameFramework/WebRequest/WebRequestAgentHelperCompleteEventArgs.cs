//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.WebRequest
{
    public sealed class WebRequestAgentHelperCompleteEventArgs : GameFrameworkEventArgs
    {
        private byte[] mWebResponseBytes;

        public WebRequestAgentHelperCompleteEventArgs()
        {
            mWebResponseBytes = null;
        }

        public static WebRequestAgentHelperCompleteEventArgs Create(byte[] webResponseBytes)
        {
            WebRequestAgentHelperCompleteEventArgs webRequestAgentHelperCompleteEventArgs = ReferencePool.Acquire<WebRequestAgentHelperCompleteEventArgs>();
            webRequestAgentHelperCompleteEventArgs.mWebResponseBytes = webResponseBytes;
            return webRequestAgentHelperCompleteEventArgs;
        }

        public override void Clear()
        {
            mWebResponseBytes = null;
        }

        public byte[] GetWebResponseBytes()
        {
            return mWebResponseBytes;
        }
    }
}
