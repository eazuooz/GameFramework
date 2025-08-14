//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceLoader
        {
            private sealed partial class LoadResourceAgent : ITaskAgent<LoadResourceTaskBase>
            {
                private static readonly Dictionary<string, string> s_CachedResourceNames = new Dictionary<string, string>(StringComparer.Ordinal);
                private static readonly HashSet<string> s_LoadingAssetNames = new HashSet<string>(StringComparer.Ordinal);
                private static readonly HashSet<string> s_LoadingResourceNames = new HashSet<string>(StringComparer.Ordinal);

                private readonly ILoadResourceAgentHelper mHelper;
                private readonly IResourceHelper mResourceHelper;
                private readonly ResourceLoader mResourceLoader;
                private readonly string mReadOnlyPath;
                private readonly string mReadWritePath;
                private readonly DecryptResourceCallback mDecryptResourceCallback;
                private LoadResourceTaskBase mTask;

                public LoadResourceAgent(ILoadResourceAgentHelper loadResourceAgentHelper, IResourceHelper resourceHelper, ResourceLoader resourceLoader, string readOnlyPath, string readWritePath, DecryptResourceCallback decryptResourceCallback)
                {
                    if (loadResourceAgentHelper == null)
                    {
                        throw new GameFrameworkException("Load resource agent helper is invalid.");
                    }

                    if (resourceHelper == null)
                    {
                        throw new GameFrameworkException("Resource helper is invalid.");
                    }

                    if (resourceLoader == null)
                    {
                        throw new GameFrameworkException("Resource loader is invalid.");
                    }

                    if (decryptResourceCallback == null)
                    {
                        throw new GameFrameworkException("Decrypt resource callback is invalid.");
                    }

                    mHelper = loadResourceAgentHelper;
                    mResourceHelper = resourceHelper;
                    mResourceLoader = resourceLoader;
                    mReadOnlyPath = readOnlyPath;
                    mReadWritePath = readWritePath;
                    mDecryptResourceCallback = decryptResourceCallback;
                    mTask = null;
                }

                public ILoadResourceAgentHelper Helper
                {
                    get
                    {
                        return mHelper;
                    }
                }

                public LoadResourceTaskBase Task
                {
                    get
                    {
                        return mTask;
                    }
                }

                public void Initialize()
                {
                    mHelper.LoadResourceAgentHelperUpdate += OnLoadResourceAgentHelperUpdate;
                    mHelper.LoadResourceAgentHelperReadFileComplete += OnLoadResourceAgentHelperReadFileComplete;
                    mHelper.LoadResourceAgentHelperReadBytesComplete += OnLoadResourceAgentHelperReadBytesComplete;
                    mHelper.LoadResourceAgentHelperParseBytesComplete += OnLoadResourceAgentHelperParseBytesComplete;
                    mHelper.LoadResourceAgentHelperLoadComplete += OnLoadResourceAgentHelperLoadComplete;
                    mHelper.LoadResourceAgentHelperError += OnLoadResourceAgentHelperError;
                }

                public void Update(float elapseSeconds, float realElapseSeconds)
                {
                }

                public void Shutdown()
                {
                    Reset();
                    mHelper.LoadResourceAgentHelperUpdate -= OnLoadResourceAgentHelperUpdate;
                    mHelper.LoadResourceAgentHelperReadFileComplete -= OnLoadResourceAgentHelperReadFileComplete;
                    mHelper.LoadResourceAgentHelperReadBytesComplete -= OnLoadResourceAgentHelperReadBytesComplete;
                    mHelper.LoadResourceAgentHelperParseBytesComplete -= OnLoadResourceAgentHelperParseBytesComplete;
                    mHelper.LoadResourceAgentHelperLoadComplete -= OnLoadResourceAgentHelperLoadComplete;
                    mHelper.LoadResourceAgentHelperError -= OnLoadResourceAgentHelperError;
                }

                public static void Clear()
                {
                    s_CachedResourceNames.Clear();
                    s_LoadingAssetNames.Clear();
                    s_LoadingResourceNames.Clear();
                }

                public StartTaskStatus Start(LoadResourceTaskBase task)
                {
                    if (task == null)
                    {
                        throw new GameFrameworkException("Task is invalid.");
                    }

                    mTask = task;
                    mTask.StartTime = DateTime.UtcNow;
                    ResourceInfo resourceInfo = mTask.ResourceInfo;

                    if (!resourceInfo.Ready)
                    {
                        mTask.StartTime = default(DateTime);
                        return StartTaskStatus.HasToWait;
                    }

                    if (IsAssetLoading(mTask.AssetName))
                    {
                        mTask.StartTime = default(DateTime);
                        return StartTaskStatus.HasToWait;
                    }

                    if (!mTask.IsScene)
                    {
                        AssetObject assetObject = mResourceLoader.mAssetPool.Spawn(mTask.AssetName);
                        if (assetObject != null)
                        {
                            OnAssetObjectReady(assetObject);
                            return StartTaskStatus.Done;
                        }
                    }

                    foreach (string dependencyAssetName in mTask.GetDependencyAssetNames())
                    {
                        if (!mResourceLoader.mAssetPool.CanSpawn(dependencyAssetName))
                        {
                            mTask.StartTime = default(DateTime);
                            return StartTaskStatus.HasToWait;
                        }
                    }

                    string resourceName = resourceInfo.ResourceName.Name;
                    if (IsResourceLoading(resourceName))
                    {
                        mTask.StartTime = default(DateTime);
                        return StartTaskStatus.HasToWait;
                    }

                    s_LoadingAssetNames.Add(mTask.AssetName);

                    ResourceObject resourceObject = mResourceLoader.mResourcePool.Spawn(resourceName);
                    if (resourceObject != null)
                    {
                        OnResourceObjectReady(resourceObject);
                        return StartTaskStatus.CanResume;
                    }

                    s_LoadingResourceNames.Add(resourceName);

                    string fullPath = null;
                    if (!s_CachedResourceNames.TryGetValue(resourceName, out fullPath))
                    {
                        fullPath = Utility.Path.GetRegularPath(Path.Combine(resourceInfo.StorageInReadOnly ? mReadOnlyPath : mReadWritePath, resourceInfo.UseFileSystem ? resourceInfo.FileSystemName : resourceInfo.ResourceName.FullName));
                        s_CachedResourceNames.Add(resourceName, fullPath);
                    }

                    if (resourceInfo.LoadType == LoadType.LoadFromFile)
                    {
                        if (resourceInfo.UseFileSystem)
                        {
                            IFileSystem fileSystem = mResourceLoader.mResourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                            mHelper.ReadFile(fileSystem, resourceInfo.ResourceName.FullName);
                        }
                        else
                        {
                            mHelper.ReadFile(fullPath);
                        }
                    }
                    else if (resourceInfo.LoadType == LoadType.LoadFromMemory || resourceInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || resourceInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt)
                    {
                        if (resourceInfo.UseFileSystem)
                        {
                            IFileSystem fileSystem = mResourceLoader.mResourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                            mHelper.ReadBytes(fileSystem, resourceInfo.ResourceName.FullName);
                        }
                        else
                        {
                            mHelper.ReadBytes(fullPath);
                        }
                    }
                    else
                    {
                        throw new GameFrameworkException(Utility.Text.Format("Resource load type '{0}' is not supported.", resourceInfo.LoadType));
                    }

                    return StartTaskStatus.CanResume;
                }

                public void Reset()
                {
                    mHelper.Reset();
                    mTask = null;
                }

                private static bool IsAssetLoading(string assetName)
                {
                    return s_LoadingAssetNames.Contains(assetName);
                }

                private static bool IsResourceLoading(string resourceName)
                {
                    return s_LoadingResourceNames.Contains(resourceName);
                }

                private void OnAssetObjectReady(AssetObject assetObject)
                {
                    mHelper.Reset();

                    object asset = assetObject.Target;
                    if (mTask.IsScene)
                    {
                        mResourceLoader.mSceneToAssetMap.Add(mTask.AssetName, asset);
                    }

                    mTask.OnLoadAssetSuccess(this, asset, (float)(DateTime.UtcNow - mTask.StartTime).TotalSeconds);
                    mTask.Done = true;
                }

                private void OnResourceObjectReady(ResourceObject resourceObject)
                {
                    mTask.LoadMain(this, resourceObject);
                }

                private void OnError(LoadResourceStatus status, string errorMessage)
                {
                    mHelper.Reset();
                    mTask.OnLoadAssetFailure(this, status, errorMessage);
                    s_LoadingAssetNames.Remove(mTask.AssetName);
                    s_LoadingResourceNames.Remove(mTask.ResourceInfo.ResourceName.Name);
                    mTask.Done = true;
                }

                private void OnLoadResourceAgentHelperUpdate(object sender, LoadResourceAgentHelperUpdateEventArgs e)
                {
                    mTask.OnLoadAssetUpdate(this, e.Type, e.Progress);
                }

                private void OnLoadResourceAgentHelperReadFileComplete(object sender, LoadResourceAgentHelperReadFileCompleteEventArgs e)
                {
                    ResourceObject resourceObject = ResourceObject.Create(mTask.ResourceInfo.ResourceName.Name, e.Resource, mResourceHelper, mResourceLoader);
                    mResourceLoader.mResourcePool.Register(resourceObject, true);
                    s_LoadingResourceNames.Remove(mTask.ResourceInfo.ResourceName.Name);
                    OnResourceObjectReady(resourceObject);
                }

                private void OnLoadResourceAgentHelperReadBytesComplete(object sender, LoadResourceAgentHelperReadBytesCompleteEventArgs e)
                {
                    byte[] bytes = e.GetBytes();
                    ResourceInfo resourceInfo = mTask.ResourceInfo;
                    if (resourceInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || resourceInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt)
                    {
                        mDecryptResourceCallback(bytes, 0, bytes.Length, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extension, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                    }

                    mHelper.ParseBytes(bytes);
                }

                private void OnLoadResourceAgentHelperParseBytesComplete(object sender, LoadResourceAgentHelperParseBytesCompleteEventArgs e)
                {
                    ResourceObject resourceObject = ResourceObject.Create(mTask.ResourceInfo.ResourceName.Name, e.Resource, mResourceHelper, mResourceLoader);
                    mResourceLoader.mResourcePool.Register(resourceObject, true);
                    s_LoadingResourceNames.Remove(mTask.ResourceInfo.ResourceName.Name);
                    OnResourceObjectReady(resourceObject);
                }

                private void OnLoadResourceAgentHelperLoadComplete(object sender, LoadResourceAgentHelperLoadCompleteEventArgs e)
                {
                    AssetObject assetObject = null;
                    if (mTask.IsScene)
                    {
                        assetObject = mResourceLoader.mAssetPool.Spawn(mTask.AssetName);
                    }

                    if (assetObject == null)
                    {
                        List<object> dependencyAssets = mTask.GetDependencyAssets();
                        assetObject = AssetObject.Create(mTask.AssetName, e.Asset, dependencyAssets, mTask.ResourceObject.Target, mResourceHelper, mResourceLoader);
                        mResourceLoader.mAssetPool.Register(assetObject, true);
                        mResourceLoader.mAssetToResourceMap.Add(e.Asset, mTask.ResourceObject.Target);
                        foreach (object dependencyAsset in dependencyAssets)
                        {
                            object dependencyResource = null;
                            if (mResourceLoader.mAssetToResourceMap.TryGetValue(dependencyAsset, out dependencyResource))
                            {
                                mTask.ResourceObject.AddDependencyResource(dependencyResource);
                            }
                            else
                            {
                                throw new GameFrameworkException("Can not find dependency resource.");
                            }
                        }
                    }

                    s_LoadingAssetNames.Remove(mTask.AssetName);
                    OnAssetObjectReady(assetObject);
                }

                private void OnLoadResourceAgentHelperError(object sender, LoadResourceAgentHelperErrorEventArgs e)
                {
                    OnError(e.Status, e.ErrorMessage);
                }
            }
        }
    }
}
