//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Resource;
using System;
using System.Collections.Generic;

namespace GameFramework.Scene
{
    internal sealed class SceneManager : GameFrameworkModule, ISceneManager
    {
        private readonly List<string> mLoadedSceneAssetNames;
        private readonly List<string> mLoadingSceneAssetNames;
        private readonly List<string> mUnloadingSceneAssetNames;
        private readonly LoadSceneCallbacks mLoadSceneCallbacks;
        private readonly UnloadSceneCallbacks mUnloadSceneCallbacks;
        private IResourceManager mResourceManager;
        private EventHandler<LoadSceneSuccessEventArgs> mLoadSceneSuccessEventHandler;
        private EventHandler<LoadSceneFailureEventArgs> mLoadSceneFailureEventHandler;
        private EventHandler<LoadSceneUpdateEventArgs> mLoadSceneUpdateEventHandler;
        private EventHandler<LoadSceneDependencyAssetEventArgs> mLoadSceneDependencyAssetEventHandler;
        private EventHandler<UnloadSceneSuccessEventArgs> mUnloadSceneSuccessEventHandler;
        private EventHandler<UnloadSceneFailureEventArgs> mUnloadSceneFailureEventHandler;

        public SceneManager()
        {
            mLoadedSceneAssetNames = new List<string>();
            mLoadingSceneAssetNames = new List<string>();
            mUnloadingSceneAssetNames = new List<string>();
            mLoadSceneCallbacks = new LoadSceneCallbacks(LoadSceneSuccessCallback, LoadSceneFailureCallback, LoadSceneUpdateCallback, LoadSceneDependencyAssetCallback);
            mUnloadSceneCallbacks = new UnloadSceneCallbacks(UnloadSceneSuccessCallback, UnloadSceneFailureCallback);
            mResourceManager = null;
            mLoadSceneSuccessEventHandler = null;
            mLoadSceneFailureEventHandler = null;
            mLoadSceneUpdateEventHandler = null;
            mLoadSceneDependencyAssetEventHandler = null;
            mUnloadSceneSuccessEventHandler = null;
            mUnloadSceneFailureEventHandler = null;
        }

        internal override int Priority
        {
            get
            {
                return 2;
            }
        }

        public event EventHandler<LoadSceneSuccessEventArgs> LoadSceneSuccess
        {
            add
            {
                mLoadSceneSuccessEventHandler += value;
            }
            remove
            {
                mLoadSceneSuccessEventHandler -= value;
            }
        }

        public event EventHandler<LoadSceneFailureEventArgs> LoadSceneFailure
        {
            add
            {
                mLoadSceneFailureEventHandler += value;
            }
            remove
            {
                mLoadSceneFailureEventHandler -= value;
            }
        }

        public event EventHandler<LoadSceneUpdateEventArgs> LoadSceneUpdate
        {
            add
            {
                mLoadSceneUpdateEventHandler += value;
            }
            remove
            {
                mLoadSceneUpdateEventHandler -= value;
            }
        }

        public event EventHandler<LoadSceneDependencyAssetEventArgs> LoadSceneDependencyAsset
        {
            add
            {
                mLoadSceneDependencyAssetEventHandler += value;
            }
            remove
            {
                mLoadSceneDependencyAssetEventHandler -= value;
            }
        }

        public event EventHandler<UnloadSceneSuccessEventArgs> UnloadSceneSuccess
        {
            add
            {
                mUnloadSceneSuccessEventHandler += value;
            }
            remove
            {
                mUnloadSceneSuccessEventHandler -= value;
            }
        }

        public event EventHandler<UnloadSceneFailureEventArgs> UnloadSceneFailure
        {
            add
            {
                mUnloadSceneFailureEventHandler += value;
            }
            remove
            {
                mUnloadSceneFailureEventHandler -= value;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            string[] loadedSceneAssetNames = mLoadedSceneAssetNames.ToArray();
            foreach (string loadedSceneAssetName in loadedSceneAssetNames)
            {
                if (SceneIsUnloading(loadedSceneAssetName))
                {
                    continue;
                }

                UnloadScene(loadedSceneAssetName);
            }

            mLoadedSceneAssetNames.Clear();
            mLoadingSceneAssetNames.Clear();
            mUnloadingSceneAssetNames.Clear();
        }

        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            mResourceManager = resourceManager;
        }

        public bool SceneIsLoaded(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return mLoadedSceneAssetNames.Contains(sceneAssetName);
        }

        public string[] GetLoadedSceneAssetNames()
        {
            return mLoadedSceneAssetNames.ToArray();
        }

        public void GetLoadedSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(mLoadedSceneAssetNames);
        }

        public bool SceneIsLoading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return mLoadingSceneAssetNames.Contains(sceneAssetName);
        }

        public string[] GetLoadingSceneAssetNames()
        {
            return mLoadingSceneAssetNames.ToArray();
        }

        public void GetLoadingSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(mLoadingSceneAssetNames);
        }

        public bool SceneIsUnloading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return mUnloadingSceneAssetNames.Contains(sceneAssetName);
        }

        public string[] GetUnloadingSceneAssetNames()
        {
            return mUnloadingSceneAssetNames.ToArray();
        }

        public void GetUnloadingSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(mUnloadingSceneAssetNames);
        }

        public bool HasScene(string sceneAssetName)
        {
            return mResourceManager.HasAsset(sceneAssetName) != HasAssetResult.NotExist;
        }

        public void LoadScene(string sceneAssetName)
        {
            LoadScene(sceneAssetName, Constant.DefaultPriority, null);
        }

        public void LoadScene(string sceneAssetName, int priority)
        {
            LoadScene(sceneAssetName, priority, null);
        }

        public void LoadScene(string sceneAssetName, object userData)
        {
            LoadScene(sceneAssetName, Constant.DefaultPriority, userData);
        }

        public void LoadScene(string sceneAssetName, int priority, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (mResourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (SceneIsUnloading(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.", sceneAssetName));
            }

            if (SceneIsLoading(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.", sceneAssetName));
            }

            if (SceneIsLoaded(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is already loaded.", sceneAssetName));
            }

            mLoadingSceneAssetNames.Add(sceneAssetName);
            mResourceManager.LoadScene(sceneAssetName, priority, mLoadSceneCallbacks, userData);
        }

        public void UnloadScene(string sceneAssetName)
        {
            UnloadScene(sceneAssetName, null);
        }

        public void UnloadScene(string sceneAssetName, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (mResourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (SceneIsUnloading(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.", sceneAssetName));
            }

            if (SceneIsLoading(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.", sceneAssetName));
            }

            if (!SceneIsLoaded(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is not loaded yet.", sceneAssetName));
            }

            mUnloadingSceneAssetNames.Add(sceneAssetName);
            mResourceManager.UnloadScene(sceneAssetName, mUnloadSceneCallbacks, userData);
        }

        private void LoadSceneSuccessCallback(string sceneAssetName, float duration, object userData)
        {
            mLoadingSceneAssetNames.Remove(sceneAssetName);
            mLoadedSceneAssetNames.Add(sceneAssetName);
            if (mLoadSceneSuccessEventHandler != null)
            {
                LoadSceneSuccessEventArgs loadSceneSuccessEventArgs = LoadSceneSuccessEventArgs.Create(sceneAssetName, duration, userData);
                mLoadSceneSuccessEventHandler(this, loadSceneSuccessEventArgs);
                ReferencePool.Release(loadSceneSuccessEventArgs);
            }
        }

        private void LoadSceneFailureCallback(string sceneAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            mLoadingSceneAssetNames.Remove(sceneAssetName);
            string appendErrorMessage = Utility.Text.Format("Load scene failure, scene asset name '{0}', status '{1}', error message '{2}'.", sceneAssetName, status, errorMessage);
            if (mLoadSceneFailureEventHandler != null)
            {
                LoadSceneFailureEventArgs loadSceneFailureEventArgs = LoadSceneFailureEventArgs.Create(sceneAssetName, appendErrorMessage, userData);
                mLoadSceneFailureEventHandler(this, loadSceneFailureEventArgs);
                ReferencePool.Release(loadSceneFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }

        private void LoadSceneUpdateCallback(string sceneAssetName, float progress, object userData)
        {
            if (mLoadSceneUpdateEventHandler != null)
            {
                LoadSceneUpdateEventArgs loadSceneUpdateEventArgs = LoadSceneUpdateEventArgs.Create(sceneAssetName, progress, userData);
                mLoadSceneUpdateEventHandler(this, loadSceneUpdateEventArgs);
                ReferencePool.Release(loadSceneUpdateEventArgs);
            }
        }

        private void LoadSceneDependencyAssetCallback(string sceneAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            if (mLoadSceneDependencyAssetEventHandler != null)
            {
                LoadSceneDependencyAssetEventArgs loadSceneDependencyAssetEventArgs = LoadSceneDependencyAssetEventArgs.Create(sceneAssetName, dependencyAssetName, loadedCount, totalCount, userData);
                mLoadSceneDependencyAssetEventHandler(this, loadSceneDependencyAssetEventArgs);
                ReferencePool.Release(loadSceneDependencyAssetEventArgs);
            }
        }

        private void UnloadSceneSuccessCallback(string sceneAssetName, object userData)
        {
            mUnloadingSceneAssetNames.Remove(sceneAssetName);
            mLoadedSceneAssetNames.Remove(sceneAssetName);
            if (mUnloadSceneSuccessEventHandler != null)
            {
                UnloadSceneSuccessEventArgs unloadSceneSuccessEventArgs = UnloadSceneSuccessEventArgs.Create(sceneAssetName, userData);
                mUnloadSceneSuccessEventHandler(this, unloadSceneSuccessEventArgs);
                ReferencePool.Release(unloadSceneSuccessEventArgs);
            }
        }

        private void UnloadSceneFailureCallback(string sceneAssetName, object userData)
        {
            mUnloadingSceneAssetNames.Remove(sceneAssetName);
            if (mUnloadSceneFailureEventHandler != null)
            {
                UnloadSceneFailureEventArgs unloadSceneFailureEventArgs = UnloadSceneFailureEventArgs.Create(sceneAssetName, userData);
                mUnloadSceneFailureEventHandler(this, unloadSceneFailureEventArgs);
                ReferencePool.Release(unloadSceneFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(Utility.Text.Format("Unload scene failure, scene asset name '{0}'.", sceneAssetName));
        }
    }
}
