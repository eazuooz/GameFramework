//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadSceneCallbacks
    {
        private readonly LoadSceneSuccessCallback mLoadSceneSuccessCallback;
        private readonly LoadSceneFailureCallback mLoadSceneFailureCallback;
        private readonly LoadSceneUpdateCallback mLoadSceneUpdateCallback;
        private readonly LoadSceneDependencyAssetCallback mLoadSceneDependencyAssetCallback;

        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback)
            : this(loadSceneSuccessCallback, null, null, null)
        {
        }

        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback)
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, null, null)
        {
        }

        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneUpdateCallback loadSceneUpdateCallback)
            : this(loadSceneSuccessCallback, null, loadSceneUpdateCallback, null)
        {
        }

        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback)
            : this(loadSceneSuccessCallback, null, null, loadSceneDependencyAssetCallback)
        {
        }

        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback, LoadSceneUpdateCallback loadSceneUpdateCallback)
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, loadSceneUpdateCallback, null)
        {
        }

        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback, LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback)
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, null, loadSceneDependencyAssetCallback)
        {
        }

        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback, LoadSceneUpdateCallback loadSceneUpdateCallback, LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback)
        {
            if (loadSceneSuccessCallback == null)
            {
                throw new GameFrameworkException("Load scene success callback is invalid.");
            }

            mLoadSceneSuccessCallback = loadSceneSuccessCallback;
            mLoadSceneFailureCallback = loadSceneFailureCallback;
            mLoadSceneUpdateCallback = loadSceneUpdateCallback;
            mLoadSceneDependencyAssetCallback = loadSceneDependencyAssetCallback;
        }

        public LoadSceneSuccessCallback LoadSceneSuccessCallback
        {
            get
            {
                return mLoadSceneSuccessCallback;
            }
        }

        public LoadSceneFailureCallback LoadSceneFailureCallback
        {
            get
            {
                return mLoadSceneFailureCallback;
            }
        }

        public LoadSceneUpdateCallback LoadSceneUpdateCallback
        {
            get
            {
                return mLoadSceneUpdateCallback;
            }
        }

        public LoadSceneDependencyAssetCallback LoadSceneDependencyAssetCallback
        {
            get
            {
                return mLoadSceneDependencyAssetCallback;
            }
        }
    }
}
