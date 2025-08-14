//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadBytesCallbacks
    {
        private readonly LoadBytesSuccessCallback mLoadBytesSuccessCallback;
        private readonly LoadBytesFailureCallback mLoadBytesFailureCallback;

        public LoadBytesCallbacks(LoadBytesSuccessCallback loadBinarySuccessCallback)
            : this(loadBinarySuccessCallback, null)
        {
        }

        public LoadBytesCallbacks(LoadBytesSuccessCallback loadBytesSuccessCallback, LoadBytesFailureCallback loadBytesFailureCallback)
        {
            if (loadBytesSuccessCallback == null)
            {
                throw new GameFrameworkException("Load bytes success callback is invalid.");
            }

            mLoadBytesSuccessCallback = loadBytesSuccessCallback;
            mLoadBytesFailureCallback = loadBytesFailureCallback;
        }

        public LoadBytesSuccessCallback LoadBytesSuccessCallback
        {
            get
            {
                return mLoadBytesSuccessCallback;
            }
        }

        public LoadBytesFailureCallback LoadBytesFailureCallback
        {
            get
            {
                return mLoadBytesFailureCallback;
            }
        }
    }
}
