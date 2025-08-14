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
            private sealed class ResourceObject : ObjectBase
            {
                private List<object> mDependencyResources;
                private IResourceHelper mResourceHelper;
                private ResourceLoader mResourceLoader;

                public ResourceObject()
                {
                    mDependencyResources = new List<object>();
                    mResourceHelper = null;
                    mResourceLoader = null;
                }

                public override bool CustomCanReleaseFlag
                {
                    get
                    {
                        int targetReferenceCount = 0;
                        mResourceLoader.mResourceDependencyCount.TryGetValue(Target, out targetReferenceCount);
                        return base.CustomCanReleaseFlag && targetReferenceCount <= 0;
                    }
                }

                public static ResourceObject Create(string name, object target, IResourceHelper resourceHelper, ResourceLoader resourceLoader)
                {
                    if (resourceHelper == null)
                    {
                        throw new GameFrameworkException("Resource helper is invalid.");
                    }

                    if (resourceLoader == null)
                    {
                        throw new GameFrameworkException("Resource loader is invalid.");
                    }

                    ResourceObject resourceObject = ReferencePool.Acquire<ResourceObject>();
                    resourceObject.Initialize(name, target);
                    resourceObject.mResourceHelper = resourceHelper;
                    resourceObject.mResourceLoader = resourceLoader;
                    return resourceObject;
                }

                public override void Clear()
                {
                    base.Clear();
                    mDependencyResources.Clear();
                    mResourceHelper = null;
                    mResourceLoader = null;
                }

                public void AddDependencyResource(object dependencyResource)
                {
                    if (Target == dependencyResource)
                    {
                        return;
                    }

                    if (mDependencyResources.Contains(dependencyResource))
                    {
                        return;
                    }

                    mDependencyResources.Add(dependencyResource);

                    int referenceCount = 0;
                    if (mResourceLoader.mResourceDependencyCount.TryGetValue(dependencyResource, out referenceCount))
                    {
                        mResourceLoader.mResourceDependencyCount[dependencyResource] = referenceCount + 1;
                    }
                    else
                    {
                        mResourceLoader.mResourceDependencyCount.Add(dependencyResource, 1);
                    }
                }

                protected internal override void Release(bool isShutdown)
                {
                    if (!isShutdown)
                    {
                        int targetReferenceCount = 0;
                        if (mResourceLoader.mResourceDependencyCount.TryGetValue(Target, out targetReferenceCount) && targetReferenceCount > 0)
                        {
                            throw new GameFrameworkException(Utility.Text.Format("Resource target '{0}' reference count is '{1}' larger than 0.", Name, targetReferenceCount));
                        }

                        foreach (object dependencyResource in mDependencyResources)
                        {
                            int referenceCount = 0;
                            if (mResourceLoader.mResourceDependencyCount.TryGetValue(dependencyResource, out referenceCount))
                            {
                                mResourceLoader.mResourceDependencyCount[dependencyResource] = referenceCount - 1;
                            }
                            else
                            {
                                throw new GameFrameworkException(Utility.Text.Format("Resource target '{0}' dependency asset reference count is invalid.", Name));
                            }
                        }
                    }

                    mResourceLoader.mResourceDependencyCount.Remove(Target);
                    mResourceHelper.Release(Target);
                }
            }
        }
    }
}
