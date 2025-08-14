//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.ObjectPool;
using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceLoader
        {
            private sealed class AssetObject : ObjectBase
            {
                private List<object> mDependencyAssets;
                private object mResource;
                private IResourceHelper mResourceHelper;
                private ResourceLoader mResourceLoader;

                public AssetObject()
                {
                    mDependencyAssets = new List<object>();
                    mResource = null;
                    mResourceHelper = null;
                    mResourceLoader = null;
                }

                public override bool CustomCanReleaseFlag
                {
                    get
                    {
                        int targetReferenceCount = 0;
                        mResourceLoader.mAssetDependencyCount.TryGetValue(Target, out targetReferenceCount);
                        return base.CustomCanReleaseFlag && targetReferenceCount <= 0;
                    }
                }

                public static AssetObject Create(string name, object target, List<object> dependencyAssets, object resource, IResourceHelper resourceHelper, ResourceLoader resourceLoader)
                {
                    if (dependencyAssets == null)
                    {
                        throw new GameFrameworkException("Dependency assets is invalid.");
                    }

                    if (resource == null)
                    {
                        throw new GameFrameworkException("Resource is invalid.");
                    }

                    if (resourceHelper == null)
                    {
                        throw new GameFrameworkException("Resource helper is invalid.");
                    }

                    if (resourceLoader == null)
                    {
                        throw new GameFrameworkException("Resource loader is invalid.");
                    }

                    AssetObject assetObject = ReferencePool.Acquire<AssetObject>();
                    assetObject.Initialize(name, target);
                    assetObject.mDependencyAssets.AddRange(dependencyAssets);
                    assetObject.mResource = resource;
                    assetObject.mResourceHelper = resourceHelper;
                    assetObject.mResourceLoader = resourceLoader;

                    foreach (object dependencyAsset in dependencyAssets)
                    {
                        int referenceCount = 0;
                        if (resourceLoader.mAssetDependencyCount.TryGetValue(dependencyAsset, out referenceCount))
                        {
                            resourceLoader.mAssetDependencyCount[dependencyAsset] = referenceCount + 1;
                        }
                        else
                        {
                            resourceLoader.mAssetDependencyCount.Add(dependencyAsset, 1);
                        }
                    }

                    return assetObject;
                }

                public override void Clear()
                {
                    base.Clear();
                    mDependencyAssets.Clear();
                    mResource = null;
                    mResourceHelper = null;
                    mResourceLoader = null;
                }

                protected internal override void OnUnspawn()
                {
                    base.OnUnspawn();
                    foreach (object dependencyAsset in mDependencyAssets)
                    {
                        mResourceLoader.mAssetPool.Unspawn(dependencyAsset);
                    }
                }

                protected internal override void Release(bool isShutdown)
                {
                    if (!isShutdown)
                    {
                        int targetReferenceCount = 0;
                        if (mResourceLoader.mAssetDependencyCount.TryGetValue(Target, out targetReferenceCount) && targetReferenceCount > 0)
                        {
                            throw new GameFrameworkException(Utility.Text.Format("Asset target '{0}' reference count is '{1}' larger than 0.", Name, targetReferenceCount));
                        }

                        foreach (object dependencyAsset in mDependencyAssets)
                        {
                            int referenceCount = 0;
                            if (mResourceLoader.mAssetDependencyCount.TryGetValue(dependencyAsset, out referenceCount))
                            {
                                mResourceLoader.mAssetDependencyCount[dependencyAsset] = referenceCount - 1;
                            }
                            else
                            {
                                throw new GameFrameworkException(Utility.Text.Format("Asset target '{0}' dependency asset reference count is invalid.", Name));
                            }
                        }

                        mResourceLoader.mResourcePool.Unspawn(mResource);
                    }

                    mResourceLoader.mAssetDependencyCount.Remove(Target);
                    mResourceLoader.mAssetToResourceMap.Remove(Target);
                    mResourceHelper.Release(Target);
                }
            }
        }
    }
}
