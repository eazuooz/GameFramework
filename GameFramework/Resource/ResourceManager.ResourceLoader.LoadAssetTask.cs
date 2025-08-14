//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceLoader
        {
            private sealed class LoadAssetTask : LoadResourceTaskBase
            {
                private LoadAssetCallbacks mLoadAssetCallbacks;

                public LoadAssetTask()
                {
                    mLoadAssetCallbacks = null;
                }

                public override bool IsScene
                {
                    get
                    {
                        return false;
                    }
                }

                public static LoadAssetTask Create(string assetName, Type assetType, int priority, ResourceInfo resourceInfo, string[] dependencyAssetNames, LoadAssetCallbacks loadAssetCallbacks, object userData)
                {
                    LoadAssetTask loadAssetTask = ReferencePool.Acquire<LoadAssetTask>();
                    loadAssetTask.Initialize(assetName, assetType, priority, resourceInfo, dependencyAssetNames, userData);
                    loadAssetTask.mLoadAssetCallbacks = loadAssetCallbacks;
                    return loadAssetTask;
                }

                public override void Clear()
                {
                    base.Clear();
                    mLoadAssetCallbacks = null;
                }

                public override void OnLoadAssetSuccess(LoadResourceAgent agent, object asset, float duration)
                {
                    base.OnLoadAssetSuccess(agent, asset, duration);
                    if (mLoadAssetCallbacks.LoadAssetSuccessCallback != null)
                    {
                        mLoadAssetCallbacks.LoadAssetSuccessCallback(AssetName, asset, duration, UserData);
                    }
                }

                public override void OnLoadAssetFailure(LoadResourceAgent agent, LoadResourceStatus status, string errorMessage)
                {
                    base.OnLoadAssetFailure(agent, status, errorMessage);
                    if (mLoadAssetCallbacks.LoadAssetFailureCallback != null)
                    {
                        mLoadAssetCallbacks.LoadAssetFailureCallback(AssetName, status, errorMessage, UserData);
                    }
                }

                public override void OnLoadAssetUpdate(LoadResourceAgent agent, LoadResourceProgress type, float progress)
                {
                    base.OnLoadAssetUpdate(agent, type, progress);
                    if (type == LoadResourceProgress.LoadAsset)
                    {
                        if (mLoadAssetCallbacks.LoadAssetUpdateCallback != null)
                        {
                            mLoadAssetCallbacks.LoadAssetUpdateCallback(AssetName, progress, UserData);
                        }
                    }
                }

                public override void OnLoadDependencyAsset(LoadResourceAgent agent, string dependencyAssetName, object dependencyAsset)
                {
                    base.OnLoadDependencyAsset(agent, dependencyAssetName, dependencyAsset);
                    if (mLoadAssetCallbacks.LoadAssetDependencyAssetCallback != null)
                    {
                        mLoadAssetCallbacks.LoadAssetDependencyAssetCallback(AssetName, dependencyAssetName, LoadedDependencyAssetCount, TotalDependencyAssetCount, UserData);
                    }
                }
            }
        }
    }
}
