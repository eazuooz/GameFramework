//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadBinaryCallbacks
    {
        private readonly LoadBinarySuccessCallback mLoadBinarySuccessCallback;
        private readonly LoadBinaryFailureCallback mLoadBinaryFailureCallback;

        public LoadBinaryCallbacks(LoadBinarySuccessCallback loadBinarySuccessCallback)
            : this(loadBinarySuccessCallback, null)
        {
        }

        public LoadBinaryCallbacks(LoadBinarySuccessCallback loadBinarySuccessCallback, LoadBinaryFailureCallback loadBinaryFailureCallback)
        {
            if (loadBinarySuccessCallback == null)
            {
                throw new GameFrameworkException("Load binary success callback is invalid.");
            }

            mLoadBinarySuccessCallback = loadBinarySuccessCallback;
            mLoadBinaryFailureCallback = loadBinaryFailureCallback;
        }

        public LoadBinarySuccessCallback LoadBinarySuccessCallback
        {
            get
            {
                return mLoadBinarySuccessCallback;
            }
        }

        public LoadBinaryFailureCallback LoadBinaryFailureCallback
        {
            get
            {
                return mLoadBinaryFailureCallback;
            }
        }
    }
}
