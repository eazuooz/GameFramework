//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.FileSystem;
using GameFramework.ObjectPool;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceLoader
        {
            private const int CachedHashBytesLength = 4;

            private readonly ResourceManager mResourceManager;
            private readonly TaskPool<LoadResourceTaskBase> mTaskPool;
            private readonly Dictionary<object, int> mAssetDependencyCount;
            private readonly Dictionary<object, int> mResourceDependencyCount;
            private readonly Dictionary<object, object> mAssetToResourceMap;
            private readonly Dictionary<string, object> mSceneToAssetMap;
            private readonly LoadBytesCallbacks mLoadBytesCallbacks;
            private readonly byte[] mCachedHashBytes;
            private IObjectPool<AssetObject> mAssetPool;
            private IObjectPool<ResourceObject> mResourcePool;

            public ResourceLoader(ResourceManager resourceManager)
            {
                mResourceManager = resourceManager;
                mTaskPool = new TaskPool<LoadResourceTaskBase>();
                mAssetDependencyCount = new Dictionary<object, int>();
                mResourceDependencyCount = new Dictionary<object, int>();
                mAssetToResourceMap = new Dictionary<object, object>();
                mSceneToAssetMap = new Dictionary<string, object>(StringComparer.Ordinal);
                mLoadBytesCallbacks = new LoadBytesCallbacks(OnLoadBinarySuccess, OnLoadBinaryFailure);
                mCachedHashBytes = new byte[CachedHashBytesLength];
                mAssetPool = null;
                mResourcePool = null;
            }

            public int TotalAgentCount
            {
                get
                {
                    return mTaskPool.TotalAgentCount;
                }
            }

            public int FreeAgentCount
            {
                get
                {
                    return mTaskPool.FreeAgentCount;
                }
            }

            public int WorkingAgentCount
            {
                get
                {
                    return mTaskPool.WorkingAgentCount;
                }
            }

            public int WaitingTaskCount
            {
                get
                {
                    return mTaskPool.WaitingTaskCount;
                }
            }

            public float AssetAutoReleaseInterval
            {
                get
                {
                    return mAssetPool.AutoReleaseInterval;
                }
                set
                {
                    mAssetPool.AutoReleaseInterval = value;
                }
            }

            public int AssetCapacity
            {
                get
                {
                    return mAssetPool.Capacity;
                }
                set
                {
                    mAssetPool.Capacity = value;
                }
            }

            public float AssetExpireTime
            {
                get
                {
                    return mAssetPool.ExpireTime;
                }
                set
                {
                    mAssetPool.ExpireTime = value;
                }
            }

            public int AssetPriority
            {
                get
                {
                    return mAssetPool.Priority;
                }
                set
                {
                    mAssetPool.Priority = value;
                }
            }

            public float ResourceAutoReleaseInterval
            {
                get
                {
                    return mResourcePool.AutoReleaseInterval;
                }
                set
                {
                    mResourcePool.AutoReleaseInterval = value;
                }
            }

            public int ResourceCapacity
            {
                get
                {
                    return mResourcePool.Capacity;
                }
                set
                {
                    mResourcePool.Capacity = value;
                }
            }

            public float ResourceExpireTime
            {
                get
                {
                    return mResourcePool.ExpireTime;
                }
                set
                {
                    mResourcePool.ExpireTime = value;
                }
            }

            public int ResourcePriority
            {
                get
                {
                    return mResourcePool.Priority;
                }
                set
                {
                    mResourcePool.Priority = value;
                }
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                mTaskPool.Update(elapseSeconds, realElapseSeconds);
            }

            public void Shutdown()
            {
                mTaskPool.Shutdown();
                mAssetDependencyCount.Clear();
                mResourceDependencyCount.Clear();
                mAssetToResourceMap.Clear();
                mSceneToAssetMap.Clear();
                LoadResourceAgent.Clear();
            }

            public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
            {
                mAssetPool = objectPoolManager.CreateMultiSpawnObjectPool<AssetObject>("Asset Pool");
                mResourcePool = objectPoolManager.CreateMultiSpawnObjectPool<ResourceObject>("Resource Pool");
            }

            public void AddLoadResourceAgentHelper(ILoadResourceAgentHelper loadResourceAgentHelper, IResourceHelper resourceHelper, string readOnlyPath, string readWritePath, DecryptResourceCallback decryptResourceCallback)
            {
                if (mAssetPool == null || mResourcePool == null)
                {
                    throw new GameFrameworkException("You must set object pool manager first.");
                }

                LoadResourceAgent agent = new LoadResourceAgent(loadResourceAgentHelper, resourceHelper, this, readOnlyPath, readWritePath, decryptResourceCallback ?? DefaultDecryptResourceCallback);
                mTaskPool.AddAgent(agent);
            }

            public HasAssetResult HasAsset(string assetName)
            {
                ResourceInfo resourceInfo = GetResourceInfo(assetName);
                if (resourceInfo == null)
                {
                    return HasAssetResult.NotExist;
                }

                if (!resourceInfo.Ready && mResourceManager.mResourceMode != ResourceMode.UpdatableWhilePlaying)
                {
                    return HasAssetResult.NotReady;
                }

                if (resourceInfo.UseFileSystem)
                {
                    return resourceInfo.IsLoadFromBinary ? HasAssetResult.BinaryOnFileSystem : HasAssetResult.AssetOnFileSystem;
                }
                else
                {
                    return resourceInfo.IsLoadFromBinary ? HasAssetResult.BinaryOnDisk : HasAssetResult.AssetOnDisk;
                }
            }

            public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
            {
                ResourceInfo resourceInfo = null;
                string[] dependencyAssetNames = null;
                if (!CheckAsset(assetName, out resourceInfo, out dependencyAssetNames))
                {
                    string errorMessage = Utility.Text.Format("Can not load asset '{0}'.", assetName);
                    if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                    {
                        loadAssetCallbacks.LoadAssetFailureCallback(assetName, resourceInfo != null && !resourceInfo.Ready ? LoadResourceStatus.NotReady : LoadResourceStatus.NotExist, errorMessage, userData);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (resourceInfo.IsLoadFromBinary)
                {
                    string errorMessage = Utility.Text.Format("Can not load asset '{0}' which is a binary asset.", assetName);
                    if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                    {
                        loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.TypeError, errorMessage, userData);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                LoadAssetTask mainTask = LoadAssetTask.Create(assetName, assetType, priority, resourceInfo, dependencyAssetNames, loadAssetCallbacks, userData);
                foreach (string dependencyAssetName in dependencyAssetNames)
                {
                    if (!LoadDependencyAsset(dependencyAssetName, priority, mainTask, userData))
                    {
                        string errorMessage = Utility.Text.Format("Can not load dependency asset '{0}' when load asset '{1}'.", dependencyAssetName, assetName);
                        if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                        {
                            loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.DependencyError, errorMessage, userData);
                            return;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }
                }

                mTaskPool.AddTask(mainTask);
                if (!resourceInfo.Ready)
                {
                    mResourceManager.UpdateResource(resourceInfo.ResourceName);
                }
            }

            public void UnloadAsset(object asset)
            {
                mAssetPool.Unspawn(asset);
            }

            public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData)
            {
                ResourceInfo resourceInfo = null;
                string[] dependencyAssetNames = null;
                if (!CheckAsset(sceneAssetName, out resourceInfo, out dependencyAssetNames))
                {
                    string errorMessage = Utility.Text.Format("Can not load scene '{0}'.", sceneAssetName);
                    if (loadSceneCallbacks.LoadSceneFailureCallback != null)
                    {
                        loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, resourceInfo != null && !resourceInfo.Ready ? LoadResourceStatus.NotReady : LoadResourceStatus.NotExist, errorMessage, userData);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (resourceInfo.IsLoadFromBinary)
                {
                    string errorMessage = Utility.Text.Format("Can not load scene asset '{0}' which is a binary asset.", sceneAssetName);
                    if (loadSceneCallbacks.LoadSceneFailureCallback != null)
                    {
                        loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.TypeError, errorMessage, userData);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                LoadSceneTask mainTask = LoadSceneTask.Create(sceneAssetName, priority, resourceInfo, dependencyAssetNames, loadSceneCallbacks, userData);
                foreach (string dependencyAssetName in dependencyAssetNames)
                {
                    if (!LoadDependencyAsset(dependencyAssetName, priority, mainTask, userData))
                    {
                        string errorMessage = Utility.Text.Format("Can not load dependency asset '{0}' when load scene '{1}'.", dependencyAssetName, sceneAssetName);
                        if (loadSceneCallbacks.LoadSceneFailureCallback != null)
                        {
                            loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.DependencyError, errorMessage, userData);
                            return;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }
                }

                mTaskPool.AddTask(mainTask);
                if (!resourceInfo.Ready)
                {
                    mResourceManager.UpdateResource(resourceInfo.ResourceName);
                }
            }

            public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
            {
                if (mResourceManager.mResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                object asset = null;
                if (mSceneToAssetMap.TryGetValue(sceneAssetName, out asset))
                {
                    mSceneToAssetMap.Remove(sceneAssetName);
                    mAssetPool.Unspawn(asset);
                    mAssetPool.ReleaseObject(asset);
                }
                else
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not find asset of scene '{0}'.", sceneAssetName));
                }

                mResourceManager.mResourceHelper.UnloadScene(sceneAssetName, unloadSceneCallbacks, userData);
            }

            public string GetBinaryPath(string binaryAssetName)
            {
                ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
                if (resourceInfo == null)
                {
                    return null;
                }

                if (!resourceInfo.Ready)
                {
                    return null;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return null;
                }

                if (resourceInfo.UseFileSystem)
                {
                    return null;
                }

                return Utility.Path.GetRegularPath(Path.Combine(resourceInfo.StorageInReadOnly ? mResourceManager.mReadOnlyPath : mResourceManager.mReadWritePath, resourceInfo.ResourceName.FullName));
            }

            public bool GetBinaryPath(string binaryAssetName, out bool storageInReadOnly, out bool storageInFileSystem, out string relativePath, out string fileName)
            {
                storageInReadOnly = false;
                storageInFileSystem = false;
                relativePath = null;
                fileName = null;

                ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
                if (resourceInfo == null)
                {
                    return false;
                }

                if (!resourceInfo.Ready)
                {
                    return false;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return false;
                }

                storageInReadOnly = resourceInfo.StorageInReadOnly;
                if (resourceInfo.UseFileSystem)
                {
                    storageInFileSystem = true;
                    relativePath = Utility.Text.Format("{0}.{1}", resourceInfo.FileSystemName, DefaultExtension);
                    fileName = resourceInfo.ResourceName.FullName;
                }
                else
                {
                    relativePath = resourceInfo.ResourceName.FullName;
                }

                return true;
            }

            public int GetBinaryLength(string binaryAssetName)
            {
                ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
                if (resourceInfo == null)
                {
                    return -1;
                }

                if (!resourceInfo.Ready)
                {
                    return -1;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return -1;
                }

                return resourceInfo.Length;
            }

            public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData)
            {
                ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
                if (resourceInfo == null)
                {
                    string errorMessage = Utility.Text.Format("Can not load binary '{0}' which is not exist.", binaryAssetName);
                    if (loadBinaryCallbacks.LoadBinaryFailureCallback != null)
                    {
                        loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.NotExist, errorMessage, userData);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (!resourceInfo.Ready)
                {
                    string errorMessage = Utility.Text.Format("Can not load binary '{0}' which is not ready.", binaryAssetName);
                    if (loadBinaryCallbacks.LoadBinaryFailureCallback != null)
                    {
                        loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.NotReady, errorMessage, userData);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    string errorMessage = Utility.Text.Format("Can not load binary '{0}' which is not a binary asset.", binaryAssetName);
                    if (loadBinaryCallbacks.LoadBinaryFailureCallback != null)
                    {
                        loadBinaryCallbacks.LoadBinaryFailureCallback(binaryAssetName, LoadResourceStatus.TypeError, errorMessage, userData);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (resourceInfo.UseFileSystem)
                {
                    loadBinaryCallbacks.LoadBinarySuccessCallback(binaryAssetName, LoadBinaryFromFileSystem(binaryAssetName), 0f, userData);
                }
                else
                {
                    string path = Utility.Path.GetRemotePath(Path.Combine(resourceInfo.StorageInReadOnly ? mResourceManager.mReadOnlyPath : mResourceManager.mReadWritePath, resourceInfo.ResourceName.FullName));
                    mResourceManager.mResourceHelper.LoadBytes(path, mLoadBytesCallbacks, LoadBinaryInfo.Create(binaryAssetName, resourceInfo, loadBinaryCallbacks, userData));
                }
            }

            public byte[] LoadBinaryFromFileSystem(string binaryAssetName)
            {
                ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
                if (resourceInfo == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not exist.", binaryAssetName));
                }

                if (!resourceInfo.Ready)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not ready.", binaryAssetName));
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not a binary asset.", binaryAssetName));
                }

                if (!resourceInfo.UseFileSystem)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not use file system.", binaryAssetName));
                }

                IFileSystem fileSystem = mResourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                byte[] bytes = fileSystem.ReadFile(resourceInfo.ResourceName.FullName);
                if (bytes == null)
                {
                    return null;
                }

                if (resourceInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || resourceInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                {
                    DecryptResourceCallback decryptResourceCallback = mResourceManager.mDecryptResourceCallback ?? DefaultDecryptResourceCallback;
                    decryptResourceCallback(bytes, 0, bytes.Length, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extension, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                return bytes;
            }

            public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length)
            {
                ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
                if (resourceInfo == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not exist.", binaryAssetName));
                }

                if (!resourceInfo.Ready)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not ready.", binaryAssetName));
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not a binary asset.", binaryAssetName));
                }

                if (!resourceInfo.UseFileSystem)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not use file system.", binaryAssetName));
                }

                IFileSystem fileSystem = mResourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                int bytesRead = fileSystem.ReadFile(resourceInfo.ResourceName.FullName, buffer, startIndex, length);
                if (resourceInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || resourceInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                {
                    DecryptResourceCallback decryptResourceCallback = mResourceManager.mDecryptResourceCallback ?? DefaultDecryptResourceCallback;
                    decryptResourceCallback(buffer, startIndex, bytesRead, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extension, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                return bytesRead;
            }

            public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, int length)
            {
                ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
                if (resourceInfo == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not exist.", binaryAssetName));
                }

                if (!resourceInfo.Ready)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not ready.", binaryAssetName));
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not a binary asset.", binaryAssetName));
                }

                if (!resourceInfo.UseFileSystem)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not use file system.", binaryAssetName));
                }

                IFileSystem fileSystem = mResourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                byte[] bytes = fileSystem.ReadFileSegment(resourceInfo.ResourceName.FullName, offset, length);
                if (bytes == null)
                {
                    return null;
                }

                if (resourceInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || resourceInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                {
                    DecryptResourceCallback decryptResourceCallback = mResourceManager.mDecryptResourceCallback ?? DefaultDecryptResourceCallback;
                    decryptResourceCallback(bytes, 0, bytes.Length, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extension, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                return bytes;
            }

            public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int startIndex, int length)
            {
                ResourceInfo resourceInfo = GetResourceInfo(binaryAssetName);
                if (resourceInfo == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not exist.", binaryAssetName));
                }

                if (!resourceInfo.Ready)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not ready.", binaryAssetName));
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not a binary asset.", binaryAssetName));
                }

                if (!resourceInfo.UseFileSystem)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not load binary '{0}' from file system which is not use file system.", binaryAssetName));
                }

                IFileSystem fileSystem = mResourceManager.GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                int bytesRead = fileSystem.ReadFileSegment(resourceInfo.ResourceName.FullName, offset, buffer, startIndex, length);
                if (resourceInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || resourceInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                {
                    DecryptResourceCallback decryptResourceCallback = mResourceManager.mDecryptResourceCallback ?? DefaultDecryptResourceCallback;
                    decryptResourceCallback(buffer, startIndex, bytesRead, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extension, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                return bytesRead;
            }

            public TaskInfo[] GetAllLoadAssetInfos()
            {
                return mTaskPool.GetAllTaskInfos();
            }

            public void GetAllLoadAssetInfos(List<TaskInfo> results)
            {
                mTaskPool.GetAllTaskInfos(results);
            }

            private bool LoadDependencyAsset(string assetName, int priority, LoadResourceTaskBase mainTask, object userData)
            {
                if (mainTask == null)
                {
                    throw new GameFrameworkException("Main task is invalid.");
                }

                ResourceInfo resourceInfo = null;
                string[] dependencyAssetNames = null;
                if (!CheckAsset(assetName, out resourceInfo, out dependencyAssetNames))
                {
                    return false;
                }

                if (resourceInfo.IsLoadFromBinary)
                {
                    return false;
                }

                LoadDependencyAssetTask dependencyTask = LoadDependencyAssetTask.Create(assetName, priority, resourceInfo, dependencyAssetNames, mainTask, userData);
                foreach (string dependencyAssetName in dependencyAssetNames)
                {
                    if (!LoadDependencyAsset(dependencyAssetName, priority, dependencyTask, userData))
                    {
                        return false;
                    }
                }

                mTaskPool.AddTask(dependencyTask);
                if (!resourceInfo.Ready)
                {
                    mResourceManager.UpdateResource(resourceInfo.ResourceName);
                }

                return true;
            }

            private ResourceInfo GetResourceInfo(string assetName)
            {
                if (string.IsNullOrEmpty(assetName))
                {
                    return null;
                }

                AssetInfo assetInfo = mResourceManager.GetAssetInfo(assetName);
                if (assetInfo == null)
                {
                    return null;
                }

                return mResourceManager.GetResourceInfo(assetInfo.ResourceName);
            }

            private bool CheckAsset(string assetName, out ResourceInfo resourceInfo, out string[] dependencyAssetNames)
            {
                resourceInfo = null;
                dependencyAssetNames = null;

                if (string.IsNullOrEmpty(assetName))
                {
                    return false;
                }

                AssetInfo assetInfo = mResourceManager.GetAssetInfo(assetName);
                if (assetInfo == null)
                {
                    return false;
                }

                resourceInfo = mResourceManager.GetResourceInfo(assetInfo.ResourceName);
                if (resourceInfo == null)
                {
                    return false;
                }

                dependencyAssetNames = assetInfo.GetDependencyAssetNames();
                return mResourceManager.mResourceMode == ResourceMode.UpdatableWhilePlaying ? true : resourceInfo.Ready;
            }

            private void DefaultDecryptResourceCallback(byte[] bytes, int startIndex, int count, string name, string variant, string extension, bool storageInReadOnly, string fileSystem, byte loadType, int length, int hashCode)
            {
                Utility.Converter.GetBytes(hashCode, mCachedHashBytes);
                switch ((LoadType)loadType)
                {
                    case LoadType.LoadFromMemoryAndQuickDecrypt:
                    case LoadType.LoadFromBinaryAndQuickDecrypt:
                        Utility.Encryption.GetQuickSelfXorBytes(bytes, mCachedHashBytes);
                        break;

                    case LoadType.LoadFromMemoryAndDecrypt:
                    case LoadType.LoadFromBinaryAndDecrypt:
                        Utility.Encryption.GetSelfXorBytes(bytes, mCachedHashBytes);
                        break;

                    default:
                        throw new GameFrameworkException("Not supported load type when decrypt resource.");
                }

                Array.Clear(mCachedHashBytes, 0, CachedHashBytesLength);
            }

            private void OnLoadBinarySuccess(string fileUri, byte[] bytes, float duration, object userData)
            {
                LoadBinaryInfo loadBinaryInfo = (LoadBinaryInfo)userData;
                if (loadBinaryInfo == null)
                {
                    throw new GameFrameworkException("Load binary info is invalid.");
                }

                ResourceInfo resourceInfo = loadBinaryInfo.ResourceInfo;
                if (resourceInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || resourceInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                {
                    DecryptResourceCallback decryptResourceCallback = mResourceManager.mDecryptResourceCallback ?? DefaultDecryptResourceCallback;
                    decryptResourceCallback(bytes, 0, bytes.Length, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extension, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                loadBinaryInfo.LoadBinaryCallbacks.LoadBinarySuccessCallback(loadBinaryInfo.BinaryAssetName, bytes, duration, loadBinaryInfo.UserData);
                ReferencePool.Release(loadBinaryInfo);
            }

            private void OnLoadBinaryFailure(string fileUri, string errorMessage, object userData)
            {
                LoadBinaryInfo loadBinaryInfo = (LoadBinaryInfo)userData;
                if (loadBinaryInfo == null)
                {
                    throw new GameFrameworkException("Load binary info is invalid.");
                }

                if (loadBinaryInfo.LoadBinaryCallbacks.LoadBinaryFailureCallback != null)
                {
                    loadBinaryInfo.LoadBinaryCallbacks.LoadBinaryFailureCallback(loadBinaryInfo.BinaryAssetName, LoadResourceStatus.AssetError, errorMessage, loadBinaryInfo.UserData);
                }

                ReferencePool.Release(loadBinaryInfo);
            }
        }
    }
}
