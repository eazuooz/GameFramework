//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class UnloadSceneCallbacks
    {
        private readonly UnloadSceneSuccessCallback mUnloadSceneSuccessCallback;
        private readonly UnloadSceneFailureCallback mUnloadSceneFailureCallback;

        public UnloadSceneCallbacks(UnloadSceneSuccessCallback unloadSceneSuccessCallback)
            : this(unloadSceneSuccessCallback, null)
        {
        }

        public UnloadSceneCallbacks(UnloadSceneSuccessCallback unloadSceneSuccessCallback, UnloadSceneFailureCallback unloadSceneFailureCallback)
        {
            if (unloadSceneSuccessCallback == null)
            {
                throw new GameFrameworkException("Unload scene success callback is invalid.");
            }

            mUnloadSceneSuccessCallback = unloadSceneSuccessCallback;
            mUnloadSceneFailureCallback = unloadSceneFailureCallback;
        }

        public UnloadSceneSuccessCallback UnloadSceneSuccessCallback
        {
            get
            {
                return mUnloadSceneSuccessCallback;
            }
        }

        public UnloadSceneFailureCallback UnloadSceneFailureCallback
        {
            get
            {
                return mUnloadSceneFailureCallback;
            }
        }
    }
}
