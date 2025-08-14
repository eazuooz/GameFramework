//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceLoader
        {
            private sealed class LoadSceneTask : LoadResourceTaskBase
            {
                private LoadSceneCallbacks mLoadSceneCallbacks;

                public LoadSceneTask()
                {
                    mLoadSceneCallbacks = null;
                }

                public override bool IsScene
                {
                    get
                    {
                        return true;
                    }
                }

                public static LoadSceneTask Create(string sceneAssetName, int priority, ResourceInfo resourceInfo, string[] dependencyAssetNames, LoadSceneCallbacks loadSceneCallbacks, object userData)
                {
                    LoadSceneTask loadSceneTask = ReferencePool.Acquire<LoadSceneTask>();
                    loadSceneTask.Initialize(sceneAssetName, null, priority, resourceInfo, dependencyAssetNames, userData);
                    loadSceneTask.mLoadSceneCallbacks = loadSceneCallbacks;
                    return loadSceneTask;
                }

                public override void Clear()
                {
                    base.Clear();
                    mLoadSceneCallbacks = null;
                }

                public override void OnLoadAssetSuccess(LoadResourceAgent agent, object asset, float duration)
                {
                    base.OnLoadAssetSuccess(agent, asset, duration);
                    if (mLoadSceneCallbacks.LoadSceneSuccessCallback != null)
                    {
                        mLoadSceneCallbacks.LoadSceneSuccessCallback(AssetName, duration, UserData);
                    }
                }

                public override void OnLoadAssetFailure(LoadResourceAgent agent, LoadResourceStatus status, string errorMessage)
                {
                    base.OnLoadAssetFailure(agent, status, errorMessage);
                    if (mLoadSceneCallbacks.LoadSceneFailureCallback != null)
                    {
                        mLoadSceneCallbacks.LoadSceneFailureCallback(AssetName, status, errorMessage, UserData);
                    }
                }

                public override void OnLoadAssetUpdate(LoadResourceAgent agent, LoadResourceProgress type, float progress)
                {
                    base.OnLoadAssetUpdate(agent, type, progress);
                    if (type == LoadResourceProgress.LoadScene)
                    {
                        if (mLoadSceneCallbacks.LoadSceneUpdateCallback != null)
                        {
                            mLoadSceneCallbacks.LoadSceneUpdateCallback(AssetName, progress, UserData);
                        }
                    }
                }

                public override void OnLoadDependencyAsset(LoadResourceAgent agent, string dependencyAssetName, object dependencyAsset)
                {
                    base.OnLoadDependencyAsset(agent, dependencyAssetName, dependencyAsset);
                    if (mLoadSceneCallbacks.LoadSceneDependencyAssetCallback != null)
                    {
                        mLoadSceneCallbacks.LoadSceneDependencyAssetCallback(AssetName, dependencyAssetName, LoadedDependencyAssetCount, TotalDependencyAssetCount, UserData);
                    }
                }
            }
        }
    }
}
