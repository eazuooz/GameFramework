//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class UpdateVersionListCallbacks
    {
        private readonly UpdateVersionListSuccessCallback mUpdateVersionListSuccessCallback;
        private readonly UpdateVersionListFailureCallback mUpdateVersionListFailureCallback;

        public UpdateVersionListCallbacks(UpdateVersionListSuccessCallback updateVersionListSuccessCallback)
            : this(updateVersionListSuccessCallback, null)
        {
        }

        public UpdateVersionListCallbacks(UpdateVersionListSuccessCallback updateVersionListSuccessCallback, UpdateVersionListFailureCallback updateVersionListFailureCallback)
        {
            if (updateVersionListSuccessCallback == null)
            {
                throw new GameFrameworkException("Update version list success callback is invalid.");
            }

            mUpdateVersionListSuccessCallback = updateVersionListSuccessCallback;
            mUpdateVersionListFailureCallback = updateVersionListFailureCallback;
        }

        public UpdateVersionListSuccessCallback UpdateVersionListSuccessCallback
        {
            get
            {
                return mUpdateVersionListSuccessCallback;
            }
        }

        public UpdateVersionListFailureCallback UpdateVersionListFailureCallback
        {
            get
            {
                return mUpdateVersionListFailureCallback;
            }
        }
    }
}
