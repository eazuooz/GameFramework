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
        private sealed partial class ResourceChecker
        {
            private readonly ResourceManager mResourceManager;
            private readonly Dictionary<ResourceName, CheckInfo> mCheckInfos;
            private string mCurrentVariant;
            private bool mIgnoreOtherVariant;
            private bool mUpdatableVersionListReady;
            private bool mReadOnlyVersionListReady;
            private bool mReadWriteVersionListReady;

            public GameFrameworkAction<ResourceName, string, LoadType, int, int, int, int> ResourceNeedUpdate;
            public GameFrameworkAction<int, int, int, long, long> ResourceCheckComplete;

            public ResourceChecker(ResourceManager resourceManager)
            {
                mResourceManager = resourceManager;
                mCheckInfos = new Dictionary<ResourceName, CheckInfo>();
                mCurrentVariant = null;
                mIgnoreOtherVariant = false;
                mUpdatableVersionListReady = false;
                mReadOnlyVersionListReady = false;
                mReadWriteVersionListReady = false;

                ResourceNeedUpdate = null;
                ResourceCheckComplete = null;
            }

            public void Shutdown()
            {
                mCheckInfos.Clear();
            }

            public void CheckResources(string currentVariant, bool ignoreOtherVariant)
            {
                if (mResourceManager.mResourceHelper == null)
                {
                    throw new GameFrameworkException("Resource helper is invalid.");
                }

                if (string.IsNullOrEmpty(mResourceManager.mReadOnlyPath))
                {
                    throw new GameFrameworkException("Read-only path is invalid.");
                }

                if (string.IsNullOrEmpty(mResourceManager.mReadWritePath))
                {
                    throw new GameFrameworkException("Read-write path is invalid.");
                }

                mCurrentVariant = currentVariant;
                mIgnoreOtherVariant = ignoreOtherVariant;
                mResourceManager.mResourceHelper.LoadBytes(Utility.Path.GetRemotePath(Path.Combine(mResourceManager.mReadWritePath, RemoteVersionListFileName)), new LoadBytesCallbacks(OnLoadUpdatableVersionListSuccess, OnLoadUpdatableVersionListFailure), null);
                mResourceManager.mResourceHelper.LoadBytes(Utility.Path.GetRemotePath(Path.Combine(mResourceManager.mReadOnlyPath, LocalVersionListFileName)), new LoadBytesCallbacks(OnLoadReadOnlyVersionListSuccess, OnLoadReadOnlyVersionListFailure), null);
                mResourceManager.mResourceHelper.LoadBytes(Utility.Path.GetRemotePath(Path.Combine(mResourceManager.mReadWritePath, LocalVersionListFileName)), new LoadBytesCallbacks(OnLoadReadWriteVersionListSuccess, OnLoadReadWriteVersionListFailure), null);
            }

            private void SetCachedFileSystemName(ResourceName resourceName, string fileSystemName)
            {
                GetOrAddCheckInfo(resourceName).SetCachedFileSystemName(fileSystemName);
            }

            private void SetVersionInfo(ResourceName resourceName, LoadType loadType, int length, int hashCode, int compressedLength, int compressedHashCode)
            {
                GetOrAddCheckInfo(resourceName).SetVersionInfo(loadType, length, hashCode, compressedLength, compressedHashCode);
            }

            private void SetReadOnlyInfo(ResourceName resourceName, LoadType loadType, int length, int hashCode)
            {
                GetOrAddCheckInfo(resourceName).SetReadOnlyInfo(loadType, length, hashCode);
            }

            private void SetReadWriteInfo(ResourceName resourceName, LoadType loadType, int length, int hashCode)
            {
                GetOrAddCheckInfo(resourceName).SetReadWriteInfo(loadType, length, hashCode);
            }

            private CheckInfo GetOrAddCheckInfo(ResourceName resourceName)
            {
                CheckInfo checkInfo = null;
                if (mCheckInfos.TryGetValue(resourceName, out checkInfo))
                {
                    return checkInfo;
                }

                checkInfo = new CheckInfo(resourceName);
                mCheckInfos.Add(checkInfo.ResourceName, checkInfo);

                return checkInfo;
            }

            private void RefreshCheckInfoStatus()
            {
                if (!mUpdatableVersionListReady || !mReadOnlyVersionListReady || !mReadWriteVersionListReady)
                {
                    return;
                }

                int movedCount = 0;
                int removedCount = 0;
                int updateCount = 0;
                long updateTotalLength = 0L;
                long updateTotalCompressedLength = 0L;
                foreach (KeyValuePair<ResourceName, CheckInfo> checkInfo in mCheckInfos)
                {
                    CheckInfo ci = checkInfo.Value;
                    ci.RefreshStatus(mCurrentVariant, mIgnoreOtherVariant);
                    if (ci.Status == CheckInfo.CheckStatus.StorageInReadOnly)
                    {
                        mResourceManager.mResourceInfos.Add(ci.ResourceName, new ResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode, ci.CompressedLength, true, true));
                    }
                    else if (ci.Status == CheckInfo.CheckStatus.StorageInReadWrite)
                    {
                        if (ci.NeedMoveToDisk || ci.NeedMoveToFileSystem)
                        {
                            movedCount++;
                            string resourceFullName = ci.ResourceName.FullName;
                            string resourcePath = Utility.Path.GetRegularPath(Path.Combine(mResourceManager.mReadWritePath, resourceFullName));
                            if (ci.NeedMoveToDisk)
                            {
                                IFileSystem fileSystem = mResourceManager.GetFileSystem(ci.ReadWriteFileSystemName, false);
                                if (!fileSystem.SaveAsFile(resourceFullName, resourcePath))
                                {
                                    throw new GameFrameworkException(Utility.Text.Format("Save as file '{0}' to '{1}' from file system '{2}' error.", resourceFullName, resourcePath, fileSystem.FullPath));
                                }

                                fileSystem.DeleteFile(resourceFullName);
                            }

                            if (ci.NeedMoveToFileSystem)
                            {
                                IFileSystem fileSystem = mResourceManager.GetFileSystem(ci.FileSystemName, false);
                                if (!fileSystem.WriteFile(resourceFullName, resourcePath))
                                {
                                    throw new GameFrameworkException(Utility.Text.Format("Write resource '{0}' to file system '{1}' error.", resourceFullName, fileSystem.FullPath));
                                }

                                if (File.Exists(resourcePath))
                                {
                                    File.Delete(resourcePath);
                                }
                            }
                        }

                        mResourceManager.mResourceInfos.Add(ci.ResourceName, new ResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode, ci.CompressedLength, false, true));
                        mResourceManager.mReadWriteResourceInfos.Add(ci.ResourceName, new ReadWriteResourceInfo(ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode));
                    }
                    else if (ci.Status == CheckInfo.CheckStatus.Update)
                    {
                        mResourceManager.mResourceInfos.Add(ci.ResourceName, new ResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode, ci.CompressedLength, false, false));
                        updateCount++;
                        updateTotalLength += ci.Length;
                        updateTotalCompressedLength += ci.CompressedLength;
                        if (ResourceNeedUpdate != null)
                        {
                            ResourceNeedUpdate(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Length, ci.HashCode, ci.CompressedLength, ci.CompressedHashCode);
                        }
                    }
                    else if (ci.Status == CheckInfo.CheckStatus.Unavailable || ci.Status == CheckInfo.CheckStatus.Disuse)
                    {
                        // Do nothing.
                    }
                    else
                    {
                        throw new GameFrameworkException(Utility.Text.Format("Check resources '{0}' error with unknown status.", ci.ResourceName.FullName));
                    }

                    if (ci.NeedRemove)
                    {
                        removedCount++;
                        if (ci.ReadWriteUseFileSystem)
                        {
                            IFileSystem fileSystem = mResourceManager.GetFileSystem(ci.ReadWriteFileSystemName, false);
                            fileSystem.DeleteFile(ci.ResourceName.FullName);
                        }
                        else
                        {
                            string resourcePath = Utility.Path.GetRegularPath(Path.Combine(mResourceManager.mReadWritePath, ci.ResourceName.FullName));
                            if (File.Exists(resourcePath))
                            {
                                File.Delete(resourcePath);
                            }
                        }
                    }
                }

                if (movedCount > 0 || removedCount > 0)
                {
                    RemoveEmptyFileSystems();
                    Utility.Path.RemoveEmptyDirectory(mResourceManager.mReadWritePath);
                }

                if (ResourceCheckComplete != null)
                {
                    ResourceCheckComplete(movedCount, removedCount, updateCount, updateTotalLength, updateTotalCompressedLength);
                }
            }

            private void RemoveEmptyFileSystems()
            {
                List<string> removedFileSystemNames = null;
                foreach (KeyValuePair<string, IFileSystem> fileSystem in mResourceManager.mReadWriteFileSystems)
                {
                    if (fileSystem.Value.FileCount <= 0)
                    {
                        if (removedFileSystemNames == null)
                        {
                            removedFileSystemNames = new List<string>();
                        }

                        mResourceManager.mFileSystemManager.DestroyFileSystem(fileSystem.Value, true);
                        removedFileSystemNames.Add(fileSystem.Key);
                    }
                }

                if (removedFileSystemNames != null)
                {
                    foreach (string removedFileSystemName in removedFileSystemNames)
                    {
                        mResourceManager.mReadWriteFileSystems.Remove(removedFileSystemName);
                    }
                }
            }

            private void OnLoadUpdatableVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
            {
                if (mUpdatableVersionListReady)
                {
                    throw new GameFrameworkException("Updatable version list has been parsed.");
                }

                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(bytes, false);
                    UpdatableVersionList versionList = mResourceManager.mUpdatableVersionListSerializer.Deserialize(memoryStream);
                    if (!versionList.IsValid)
                    {
                        throw new GameFrameworkException("Deserialize updatable version list failure.");
                    }

                    UpdatableVersionList.Asset[] assets = versionList.GetAssets();
                    UpdatableVersionList.Resource[] resources = versionList.GetResources();
                    UpdatableVersionList.FileSystem[] fileSystems = versionList.GetFileSystems();
                    UpdatableVersionList.ResourceGroup[] resourceGroups = versionList.GetResourceGroups();
                    mResourceManager.mApplicableGameVersion = versionList.ApplicableGameVersion;
                    mResourceManager.mInternalResourceVersion = versionList.InternalResourceVersion;
                    mResourceManager.mAssetInfos = new Dictionary<string, AssetInfo>(assets.Length, StringComparer.Ordinal);
                    mResourceManager.mResourceInfos = new Dictionary<ResourceName, ResourceInfo>(resources.Length, new ResourceNameComparer());
                    mResourceManager.mReadWriteResourceInfos = new SortedDictionary<ResourceName, ReadWriteResourceInfo>(new ResourceNameComparer());
                    ResourceGroup defaultResourceGroup = mResourceManager.GetOrAddResourceGroup(string.Empty);

                    foreach (UpdatableVersionList.FileSystem fileSystem in fileSystems)
                    {
                        int[] resourceIndexes = fileSystem.GetResourceIndexes();
                        foreach (int resourceIndex in resourceIndexes)
                        {
                            UpdatableVersionList.Resource resource = resources[resourceIndex];
                            if (resource.Variant != null && resource.Variant != mCurrentVariant)
                            {
                                continue;
                            }

                            SetCachedFileSystemName(new ResourceName(resource.Name, resource.Variant, resource.Extension), fileSystem.Name);
                        }
                    }

                    foreach (UpdatableVersionList.Resource resource in resources)
                    {
                        if (resource.Variant != null && resource.Variant != mCurrentVariant)
                        {
                            continue;
                        }

                        ResourceName resourceName = new ResourceName(resource.Name, resource.Variant, resource.Extension);
                        int[] assetIndexes = resource.GetAssetIndexes();
                        foreach (int assetIndex in assetIndexes)
                        {
                            UpdatableVersionList.Asset asset = assets[assetIndex];
                            int[] dependencyAssetIndexes = asset.GetDependencyAssetIndexes();
                            int index = 0;
                            string[] dependencyAssetNames = new string[dependencyAssetIndexes.Length];
                            foreach (int dependencyAssetIndex in dependencyAssetIndexes)
                            {
                                dependencyAssetNames[index++] = assets[dependencyAssetIndex].Name;
                            }

                            mResourceManager.mAssetInfos.Add(asset.Name, new AssetInfo(asset.Name, resourceName, dependencyAssetNames));
                        }

                        SetVersionInfo(resourceName, (LoadType)resource.LoadType, resource.Length, resource.HashCode, resource.CompressedLength, resource.CompressedHashCode);
                        defaultResourceGroup.AddResource(resourceName, resource.Length, resource.CompressedLength);
                    }

                    foreach (UpdatableVersionList.ResourceGroup resourceGroup in resourceGroups)
                    {
                        ResourceGroup group = mResourceManager.GetOrAddResourceGroup(resourceGroup.Name);
                        int[] resourceIndexes = resourceGroup.GetResourceIndexes();
                        foreach (int resourceIndex in resourceIndexes)
                        {
                            UpdatableVersionList.Resource resource = resources[resourceIndex];
                            if (resource.Variant != null && resource.Variant != mCurrentVariant)
                            {
                                continue;
                            }

                            group.AddResource(new ResourceName(resource.Name, resource.Variant, resource.Extension), resource.Length, resource.CompressedLength);
                        }
                    }

                    mUpdatableVersionListReady = true;
                    RefreshCheckInfoStatus();
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Utility.Text.Format("Parse updatable version list exception '{0}'.", exception), exception);
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            private void OnLoadUpdatableVersionListFailure(string fileUri, string errorMessage, object userData)
            {
                throw new GameFrameworkException(Utility.Text.Format("Updatable version list '{0}' is invalid, error message is '{1}'.", fileUri, string.IsNullOrEmpty(errorMessage) ? "<Empty>" : errorMessage));
            }

            private void OnLoadReadOnlyVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
            {
                if (mReadOnlyVersionListReady)
                {
                    throw new GameFrameworkException("Read-only version list has been parsed.");
                }

                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(bytes, false);
                    LocalVersionList versionList = mResourceManager.mReadOnlyVersionListSerializer.Deserialize(memoryStream);
                    if (!versionList.IsValid)
                    {
                        throw new GameFrameworkException("Deserialize read-only version list failure.");
                    }

                    LocalVersionList.Resource[] resources = versionList.GetResources();
                    LocalVersionList.FileSystem[] fileSystems = versionList.GetFileSystems();

                    foreach (LocalVersionList.FileSystem fileSystem in fileSystems)
                    {
                        int[] resourceIndexes = fileSystem.GetResourceIndexes();
                        foreach (int resourceIndex in resourceIndexes)
                        {
                            LocalVersionList.Resource resource = resources[resourceIndex];
                            SetCachedFileSystemName(new ResourceName(resource.Name, resource.Variant, resource.Extension), fileSystem.Name);
                        }
                    }

                    foreach (LocalVersionList.Resource resource in resources)
                    {
                        SetReadOnlyInfo(new ResourceName(resource.Name, resource.Variant, resource.Extension), (LoadType)resource.LoadType, resource.Length, resource.HashCode);
                    }

                    mReadOnlyVersionListReady = true;
                    RefreshCheckInfoStatus();
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Utility.Text.Format("Parse read-only version list exception '{0}'.", exception), exception);
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            private void OnLoadReadOnlyVersionListFailure(string fileUri, string errorMessage, object userData)
            {
                if (mReadOnlyVersionListReady)
                {
                    throw new GameFrameworkException("Read-only version list has been parsed.");
                }

                mReadOnlyVersionListReady = true;
                RefreshCheckInfoStatus();
            }

            private void OnLoadReadWriteVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
            {
                if (mReadWriteVersionListReady)
                {
                    throw new GameFrameworkException("Read-write version list has been parsed.");
                }

                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(bytes, false);
                    LocalVersionList versionList = mResourceManager.mReadWriteVersionListSerializer.Deserialize(memoryStream);
                    if (!versionList.IsValid)
                    {
                        throw new GameFrameworkException("Deserialize read-write version list failure.");
                    }

                    LocalVersionList.Resource[] resources = versionList.GetResources();
                    LocalVersionList.FileSystem[] fileSystems = versionList.GetFileSystems();

                    foreach (LocalVersionList.FileSystem fileSystem in fileSystems)
                    {
                        int[] resourceIndexes = fileSystem.GetResourceIndexes();
                        foreach (int resourceIndex in resourceIndexes)
                        {
                            LocalVersionList.Resource resource = resources[resourceIndex];
                            SetCachedFileSystemName(new ResourceName(resource.Name, resource.Variant, resource.Extension), fileSystem.Name);
                        }
                    }

                    foreach (LocalVersionList.Resource resource in resources)
                    {
                        SetReadWriteInfo(new ResourceName(resource.Name, resource.Variant, resource.Extension), (LoadType)resource.LoadType, resource.Length, resource.HashCode);
                    }

                    mReadWriteVersionListReady = true;
                    RefreshCheckInfoStatus();
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Utility.Text.Format("Parse read-write version list exception '{0}'.", exception), exception);
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            private void OnLoadReadWriteVersionListFailure(string fileUri, string errorMessage, object userData)
            {
                if (mReadWriteVersionListReady)
                {
                    throw new GameFrameworkException("Read-write version list has been parsed.");
                }

                mReadWriteVersionListReady = true;
                RefreshCheckInfoStatus();
            }
        }
    }
}
