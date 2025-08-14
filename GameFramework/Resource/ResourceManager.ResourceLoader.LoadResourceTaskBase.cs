//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceLoader
        {
            private abstract class LoadResourceTaskBase : TaskBase
            {
                private static int s_Serial = 0;

                private string mAssetName;
                private Type mAssetType;
                private ResourceInfo mResourceInfo;
                private string[] mDependencyAssetNames;
                private readonly List<object> mDependencyAssets;
                private ResourceObject mResourceObject;
                private DateTime mStartTime;
                private int mTotalDependencyAssetCount;

                public LoadResourceTaskBase()
                {
                    mAssetName = null;
                    mAssetType = null;
                    mResourceInfo = null;
                    mDependencyAssetNames = null;
                    mDependencyAssets = new List<object>();
                    mResourceObject = null;
                    mStartTime = default(DateTime);
                    mTotalDependencyAssetCount = 0;
                }

                public string AssetName
                {
                    get
                    {
                        return mAssetName;
                    }
                }

                public Type AssetType
                {
                    get
                    {
                        return mAssetType;
                    }
                }

                public ResourceInfo ResourceInfo
                {
                    get
                    {
                        return mResourceInfo;
                    }
                }

                public ResourceObject ResourceObject
                {
                    get
                    {
                        return mResourceObject;
                    }
                }

                public abstract bool IsScene
                {
                    get;
                }

                public DateTime StartTime
                {
                    get
                    {
                        return mStartTime;
                    }
                    set
                    {
                        mStartTime = value;
                    }
                }

                public int LoadedDependencyAssetCount
                {
                    get
                    {
                        return mDependencyAssets.Count;
                    }
                }

                public int TotalDependencyAssetCount
                {
                    get
                    {
                        return mTotalDependencyAssetCount;
                    }
                    set
                    {
                        mTotalDependencyAssetCount = value;
                    }
                }

                public override string Description
                {
                    get
                    {
                        return mAssetName;
                    }
                }

                public override void Clear()
                {
                    base.Clear();
                    mAssetName = null;
                    mAssetType = null;
                    mResourceInfo = null;
                    mDependencyAssetNames = null;
                    mDependencyAssets.Clear();
                    mResourceObject = null;
                    mStartTime = default(DateTime);
                    mTotalDependencyAssetCount = 0;
                }

                public string[] GetDependencyAssetNames()
                {
                    return mDependencyAssetNames;
                }

                public List<object> GetDependencyAssets()
                {
                    return mDependencyAssets;
                }

                public void LoadMain(LoadResourceAgent agent, ResourceObject resourceObject)
                {
                    mResourceObject = resourceObject;
                    agent.Helper.LoadAsset(resourceObject.Target, AssetName, AssetType, IsScene);
                }

                public virtual void OnLoadAssetSuccess(LoadResourceAgent agent, object asset, float duration)
                {
                }

                public virtual void OnLoadAssetFailure(LoadResourceAgent agent, LoadResourceStatus status, string errorMessage)
                {
                }

                public virtual void OnLoadAssetUpdate(LoadResourceAgent agent, LoadResourceProgress type, float progress)
                {
                }

                public virtual void OnLoadDependencyAsset(LoadResourceAgent agent, string dependencyAssetName, object dependencyAsset)
                {
                    mDependencyAssets.Add(dependencyAsset);
                }

                protected void Initialize(string assetName, Type assetType, int priority, ResourceInfo resourceInfo, string[] dependencyAssetNames, object userData)
                {
                    Initialize(++s_Serial, null, priority, userData);
                    mAssetName = assetName;
                    mAssetType = assetType;
                    mResourceInfo = resourceInfo;
                    mDependencyAssetNames = dependencyAssetNames;
                }
            }
        }
    }
}
