//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Download;
using GameFramework.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceUpdater
        {
            private const int CachedHashBytesLength = 4;
            private const int CachedBytesLength = 0x1000;

            private readonly ResourceManager mResourceManager;
            private readonly Queue<ApplyInfo> mApplyWaitingInfo;
            private readonly List<UpdateInfo> mUpdateWaitingInfo;
            private readonly HashSet<UpdateInfo> mUpdateWaitingInfoWhilePlaying;
            private readonly Dictionary<ResourceName, UpdateInfo> mUpdateCandidateInfo;
            private readonly SortedDictionary<string, List<int>> mCachedFileSystemsForGenerateReadWriteVersionList;
            private readonly List<ResourceName> mCachedResourceNames;
            private readonly byte[] mCachedHashBytes;
            private readonly byte[] mCachedBytes;
            private IDownloadManager mDownloadManager;
            private bool mCheckResourcesComplete;
            private string mApplyingResourcePackPath;
            private FileStream mApplyingResourcePackStream;
            private ResourceGroup mUpdatingResourceGroup;
            private int mGenerateReadWriteVersionListLength;
            private int mCurrentGenerateReadWriteVersionListLength;
            private int mUpdateRetryCount;
            private bool mFailureFlag;
            private string mReadWriteVersionListFileName;
            private string mReadWriteVersionListTempFileName;

            public GameFrameworkAction<string, int, long> ResourceApplyStart;
            public GameFrameworkAction<ResourceName, string, string, int, int> ResourceApplySuccess;
            public GameFrameworkAction<ResourceName, string, string> ResourceApplyFailure;
            public GameFrameworkAction<string, bool> ResourceApplyComplete;
            public GameFrameworkAction<ResourceName, string, string, int, int, int> ResourceUpdateStart;
            public GameFrameworkAction<ResourceName, string, string, int, int> ResourceUpdateChanged;
            public GameFrameworkAction<ResourceName, string, string, int, int> ResourceUpdateSuccess;
            public GameFrameworkAction<ResourceName, string, int, int, string> ResourceUpdateFailure;
            public GameFrameworkAction<ResourceGroup, bool> ResourceUpdateComplete;
            public GameFrameworkAction ResourceUpdateAllComplete;

            public ResourceUpdater(ResourceManager resourceManager)
            {
                mResourceManager = resourceManager;
                mApplyWaitingInfo = new Queue<ApplyInfo>();
                mUpdateWaitingInfo = new List<UpdateInfo>();
                mUpdateWaitingInfoWhilePlaying = new HashSet<UpdateInfo>();
                mUpdateCandidateInfo = new Dictionary<ResourceName, UpdateInfo>();
                mCachedFileSystemsForGenerateReadWriteVersionList = new SortedDictionary<string, List<int>>(StringComparer.Ordinal);
                mCachedResourceNames = new List<ResourceName>();
                mCachedHashBytes = new byte[CachedHashBytesLength];
                mCachedBytes = new byte[CachedBytesLength];
                mDownloadManager = null;
                mCheckResourcesComplete = false;
                mApplyingResourcePackPath = null;
                mApplyingResourcePackStream = null;
                mUpdatingResourceGroup = null;
                mGenerateReadWriteVersionListLength = 0;
                mCurrentGenerateReadWriteVersionListLength = 0;
                mUpdateRetryCount = 3;
                mFailureFlag = false;
                mReadWriteVersionListFileName = Utility.Path.GetRegularPath(Path.Combine(mResourceManager.mReadWritePath, LocalVersionListFileName));
                mReadWriteVersionListTempFileName = Utility.Text.Format("{0}.{1}", mReadWriteVersionListFileName, TempExtension);

                ResourceApplyStart = null;
                ResourceApplySuccess = null;
                ResourceApplyFailure = null;
                ResourceApplyComplete = null;
                ResourceUpdateStart = null;
                ResourceUpdateChanged = null;
                ResourceUpdateSuccess = null;
                ResourceUpdateFailure = null;
                ResourceUpdateComplete = null;
                ResourceUpdateAllComplete = null;
            }

            public int GenerateReadWriteVersionListLength
            {
                get
                {
                    return mGenerateReadWriteVersionListLength;
                }
                set
                {
                    mGenerateReadWriteVersionListLength = value;
                }
            }

            public string ApplyingResourcePackPath
            {
                get
                {
                    return mApplyingResourcePackPath;
                }
            }

            public int ApplyWaitingCount
            {
                get
                {
                    return mApplyWaitingInfo.Count;
                }
            }

            public int UpdateRetryCount
            {
                get
                {
                    return mUpdateRetryCount;
                }
                set
                {
                    mUpdateRetryCount = value;
                }
            }

            public IResourceGroup UpdatingResourceGroup
            {
                get
                {
                    return mUpdatingResourceGroup;
                }
            }

            public int UpdateWaitingCount
            {
                get
                {
                    return mUpdateWaitingInfo.Count;
                }
            }

            public int UpdateWaitingWhilePlayingCount
            {
                get
                {
                    return mUpdateWaitingInfoWhilePlaying.Count;
                }
            }

            public int UpdateCandidateCount
            {
                get
                {
                    return mUpdateCandidateInfo.Count;
                }
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (mApplyingResourcePackStream != null)
                {
                    while (mApplyWaitingInfo.Count > 0)
                    {
                        ApplyInfo applyInfo = mApplyWaitingInfo.Dequeue();
                        if (ApplyResource(applyInfo))
                        {
                            return;
                        }
                    }

                    Array.Clear(mCachedBytes, 0, CachedBytesLength);
                    string resourcePackPath = mApplyingResourcePackPath;
                    mApplyingResourcePackPath = null;
                    mApplyingResourcePackStream.Dispose();
                    mApplyingResourcePackStream = null;
                    if (ResourceApplyComplete != null)
                    {
                        ResourceApplyComplete(resourcePackPath, !mFailureFlag);
                    }

                    if (mUpdateCandidateInfo.Count <= 0 && ResourceUpdateAllComplete != null)
                    {
                        ResourceUpdateAllComplete();
                    }

                    return;
                }

                if (mUpdateWaitingInfo.Count > 0)
                {
                    int freeCount = mDownloadManager.FreeAgentCount - mDownloadManager.WaitingTaskCount;
                    if (freeCount > 0)
                    {
                        for (int i = 0, count = 0; i < mUpdateWaitingInfo.Count && count < freeCount; i++)
                        {
                            if (DownloadResource(mUpdateWaitingInfo[i]))
                            {
                                count++;
                            }
                        }
                    }

                    return;
                }
            }

            public void Shutdown()
            {
                if (mDownloadManager != null)
                {
                    mDownloadManager.DownloadStart -= OnDownloadStart;
                    mDownloadManager.DownloadUpdate -= OnDownloadUpdate;
                    mDownloadManager.DownloadSuccess -= OnDownloadSuccess;
                    mDownloadManager.DownloadFailure -= OnDownloadFailure;
                }

                mUpdateWaitingInfo.Clear();
                mUpdateCandidateInfo.Clear();
                mCachedFileSystemsForGenerateReadWriteVersionList.Clear();
            }

            public void SetDownloadManager(IDownloadManager downloadManager)
            {
                if (downloadManager == null)
                {
                    throw new GameFrameworkException("Download manager is invalid.");
                }

                mDownloadManager = downloadManager;
                mDownloadManager.DownloadStart += OnDownloadStart;
                mDownloadManager.DownloadUpdate += OnDownloadUpdate;
                mDownloadManager.DownloadSuccess += OnDownloadSuccess;
                mDownloadManager.DownloadFailure += OnDownloadFailure;
            }

            public void AddResourceUpdate(ResourceName resourceName, string fileSystemName, LoadType loadType, int length, int hashCode, int compressedLength, int compressedHashCode, string resourcePath)
            {
                mUpdateCandidateInfo.Add(resourceName, new UpdateInfo(resourceName, fileSystemName, loadType, length, hashCode, compressedLength, compressedHashCode, resourcePath));
            }

            public void CheckResourceComplete(bool needGenerateReadWriteVersionList)
            {
                mCheckResourcesComplete = true;
                if (needGenerateReadWriteVersionList)
                {
                    GenerateReadWriteVersionList();
                }
            }

            public void ApplyResources(string resourcePackPath)
            {
                if (!mCheckResourcesComplete)
                {
                    throw new GameFrameworkException("You must check resources complete first.");
                }

                if (mApplyingResourcePackStream != null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("There is already a resource pack '{0}' being applied.", mApplyingResourcePackPath));
                }

                if (mUpdatingResourceGroup != null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("There is already a resource group '{0}' being updated.", mUpdatingResourceGroup.Name));
                }

                if (mUpdateWaitingInfoWhilePlaying.Count > 0)
                {
                    throw new GameFrameworkException("There are already some resources being updated while playing.");
                }

                try
                {
                    long length = 0L;
                    ResourcePackVersionList versionList = default(ResourcePackVersionList);
                    using (FileStream fileStream = new FileStream(resourcePackPath, FileMode.Open, FileAccess.Read))
                    {
                        length = fileStream.Length;
                        versionList = mResourceManager.mResourcePackVersionListSerializer.Deserialize(fileStream);
                    }

                    if (!versionList.IsValid)
                    {
                        throw new GameFrameworkException("Deserialize resource pack version list failure.");
                    }

                    if (versionList.Offset + versionList.Length != length)
                    {
                        throw new GameFrameworkException("Resource pack length is invalid.");
                    }

                    mApplyingResourcePackPath = resourcePackPath;
                    mApplyingResourcePackStream = new FileStream(resourcePackPath, FileMode.Open, FileAccess.Read);
                    mApplyingResourcePackStream.Position = versionList.Offset;
                    mFailureFlag = false;

                    long totalLength = 0L;
                    ResourcePackVersionList.Resource[] resources = versionList.GetResources();
                    foreach (ResourcePackVersionList.Resource resource in resources)
                    {
                        ResourceName resourceName = new ResourceName(resource.Name, resource.Variant, resource.Extension);
                        UpdateInfo updateInfo = null;
                        if (!mUpdateCandidateInfo.TryGetValue(resourceName, out updateInfo))
                        {
                            continue;
                        }

                        if (updateInfo.LoadType == (LoadType)resource.LoadType && updateInfo.Length == resource.Length && updateInfo.HashCode == resource.HashCode)
                        {
                            totalLength += resource.Length;
                            mApplyWaitingInfo.Enqueue(new ApplyInfo(resourceName, updateInfo.FileSystemName, (LoadType)resource.LoadType, resource.Offset, resource.Length, resource.HashCode, resource.CompressedLength, resource.CompressedHashCode, updateInfo.ResourcePath));
                        }
                    }

                    if (ResourceApplyStart != null)
                    {
                        ResourceApplyStart(mApplyingResourcePackPath, mApplyWaitingInfo.Count, totalLength);
                    }
                }
                catch (Exception exception)
                {
                    if (mApplyingResourcePackStream != null)
                    {
                        mApplyingResourcePackStream.Dispose();
                        mApplyingResourcePackStream = null;
                    }

                    throw new GameFrameworkException(Utility.Text.Format("Apply resources '{0}' with exception '{1}'.", resourcePackPath, exception), exception);
                }
            }

            public void UpdateResources(ResourceGroup resourceGroup)
            {
                if (mDownloadManager == null)
                {
                    throw new GameFrameworkException("You must set download manager first.");
                }

                if (!mCheckResourcesComplete)
                {
                    throw new GameFrameworkException("You must check resources complete first.");
                }

                if (mApplyingResourcePackStream != null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("There is already a resource pack '{0}' being applied.", mApplyingResourcePackPath));
                }

                if (mUpdatingResourceGroup != null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("There is already a resource group '{0}' being updated.", mUpdatingResourceGroup.Name));
                }

                if (string.IsNullOrEmpty(resourceGroup.Name))
                {
                    foreach (KeyValuePair<ResourceName, UpdateInfo> updateInfo in mUpdateCandidateInfo)
                    {
                        mUpdateWaitingInfo.Add(updateInfo.Value);
                    }
                }
                else
                {
                    resourceGroup.InternalGetResourceNames(mCachedResourceNames);
                    foreach (ResourceName resourceName in mCachedResourceNames)
                    {
                        UpdateInfo updateInfo = null;
                        if (!mUpdateCandidateInfo.TryGetValue(resourceName, out updateInfo))
                        {
                            continue;
                        }

                        mUpdateWaitingInfo.Add(updateInfo);
                    }

                    mCachedResourceNames.Clear();
                }

                mUpdatingResourceGroup = resourceGroup;
                mFailureFlag = false;
            }

            public void StopUpdateResources()
            {
                if (mDownloadManager == null)
                {
                    throw new GameFrameworkException("You must set download manager first.");
                }

                if (!mCheckResourcesComplete)
                {
                    throw new GameFrameworkException("You must check resources complete first.");
                }

                if (mApplyingResourcePackStream != null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("There is already a resource pack '{0}' being applied.", mApplyingResourcePackPath));
                }

                if (mUpdatingResourceGroup == null)
                {
                    throw new GameFrameworkException("There is no resource group being updated.");
                }

                mUpdateWaitingInfo.Clear();
                mUpdatingResourceGroup = null;
            }

            public void UpdateResource(ResourceName resourceName)
            {
                if (mDownloadManager == null)
                {
                    throw new GameFrameworkException("You must set download manager first.");
                }

                if (!mCheckResourcesComplete)
                {
                    throw new GameFrameworkException("You must check resources complete first.");
                }

                if (mApplyingResourcePackStream != null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("There is already a resource pack '{0}' being applied.", mApplyingResourcePackPath));
                }

                UpdateInfo updateInfo = null;
                if (mUpdateCandidateInfo.TryGetValue(resourceName, out updateInfo) && mUpdateWaitingInfoWhilePlaying.Add(updateInfo))
                {
                    DownloadResource(updateInfo);
                }
            }

            private bool ApplyResource(ApplyInfo applyInfo)
            {
                long position = mApplyingResourcePackStream.Position;
                try
                {
                    bool compressed = applyInfo.Length != applyInfo.CompressedLength || applyInfo.HashCode != applyInfo.CompressedHashCode;

                    int bytesRead = 0;
                    int bytesLeft = applyInfo.CompressedLength;
                    string directory = Path.GetDirectoryName(applyInfo.ResourcePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    mApplyingResourcePackStream.Position += applyInfo.Offset;
                    using (FileStream fileStream = new FileStream(applyInfo.ResourcePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        while ((bytesRead = mApplyingResourcePackStream.Read(mCachedBytes, 0, bytesLeft < CachedBytesLength ? bytesLeft : CachedBytesLength)) > 0)
                        {
                            bytesLeft -= bytesRead;
                            fileStream.Write(mCachedBytes, 0, bytesRead);
                        }

                        if (compressed)
                        {
                            fileStream.Position = 0L;
                            int hashCode = Utility.Verifier.GetCrc32(fileStream);
                            if (hashCode != applyInfo.CompressedHashCode)
                            {
                                if (ResourceApplyFailure != null)
                                {
                                    string errorMessage = Utility.Text.Format("Resource compressed hash code error, need '{0}', applied '{1}'.", applyInfo.CompressedHashCode, hashCode);
                                    ResourceApplyFailure(applyInfo.ResourceName, mApplyingResourcePackPath, errorMessage);
                                }

                                mFailureFlag = true;
                                return false;
                            }

                            fileStream.Position = 0L;
                            mResourceManager.PrepareCachedStream();
                            if (!Utility.Compression.Decompress(fileStream, mResourceManager.mCachedStream))
                            {
                                if (ResourceApplyFailure != null)
                                {
                                    string errorMessage = Utility.Text.Format("Unable to decompress resource '{0}'.", applyInfo.ResourcePath);
                                    ResourceApplyFailure(applyInfo.ResourceName, mApplyingResourcePackPath, errorMessage);
                                }

                                mFailureFlag = true;
                                return false;
                            }

                            fileStream.Position = 0L;
                            fileStream.SetLength(0L);
                            fileStream.Write(mResourceManager.mCachedStream.GetBuffer(), 0, (int)mResourceManager.mCachedStream.Length);
                        }
                        else
                        {
                            int hashCode = 0;
                            fileStream.Position = 0L;
                            if (applyInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || applyInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt
                                || applyInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || applyInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                            {
                                Utility.Converter.GetBytes(applyInfo.HashCode, mCachedHashBytes);
                                if (applyInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || applyInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt)
                                {
                                    hashCode = Utility.Verifier.GetCrc32(fileStream, mCachedHashBytes, Utility.Encryption.QuickEncryptLength);
                                }
                                else if (applyInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt || applyInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                                {
                                    hashCode = Utility.Verifier.GetCrc32(fileStream, mCachedHashBytes, applyInfo.Length);
                                }

                                Array.Clear(mCachedHashBytes, 0, CachedHashBytesLength);
                            }
                            else
                            {
                                hashCode = Utility.Verifier.GetCrc32(fileStream);
                            }

                            if (hashCode != applyInfo.HashCode)
                            {
                                if (ResourceApplyFailure != null)
                                {
                                    string errorMessage = Utility.Text.Format("Resource hash code error, need '{0}', applied '{1}'.", applyInfo.HashCode, hashCode);
                                    ResourceApplyFailure(applyInfo.ResourceName, mApplyingResourcePackPath, errorMessage);
                                }

                                mFailureFlag = true;
                                return false;
                            }
                        }
                    }

                    if (applyInfo.UseFileSystem)
                    {
                        IFileSystem fileSystem = mResourceManager.GetFileSystem(applyInfo.FileSystemName, false);
                        bool retVal = fileSystem.WriteFile(applyInfo.ResourceName.FullName, applyInfo.ResourcePath);
                        if (File.Exists(applyInfo.ResourcePath))
                        {
                            File.Delete(applyInfo.ResourcePath);
                        }

                        if (!retVal)
                        {
                            if (ResourceApplyFailure != null)
                            {
                                string errorMessage = Utility.Text.Format("Unable to write resource '{0}' to file system '{1}'.", applyInfo.ResourcePath, applyInfo.FileSystemName);
                                ResourceApplyFailure(applyInfo.ResourceName, mApplyingResourcePackPath, errorMessage);
                            }

                            mFailureFlag = true;
                            return false;
                        }
                    }

                    string downloadingResource = Utility.Text.Format("{0}.download", applyInfo.ResourcePath);
                    if (File.Exists(downloadingResource))
                    {
                        File.Delete(downloadingResource);
                    }

                    mUpdateCandidateInfo.Remove(applyInfo.ResourceName);
                    mResourceManager.mResourceInfos[applyInfo.ResourceName].MarkReady();
                    mResourceManager.mReadWriteResourceInfos.Add(applyInfo.ResourceName, new ReadWriteResourceInfo(applyInfo.FileSystemName, applyInfo.LoadType, applyInfo.Length, applyInfo.HashCode));
                    if (ResourceApplySuccess != null)
                    {
                        ResourceApplySuccess(applyInfo.ResourceName, applyInfo.ResourcePath, mApplyingResourcePackPath, applyInfo.Length, applyInfo.CompressedLength);
                    }

                    mCurrentGenerateReadWriteVersionListLength += applyInfo.CompressedLength;
                    if (mApplyWaitingInfo.Count <= 0 || mCurrentGenerateReadWriteVersionListLength >= mGenerateReadWriteVersionListLength)
                    {
                        GenerateReadWriteVersionList();
                        return true;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    if (ResourceApplyFailure != null)
                    {
                        ResourceApplyFailure(applyInfo.ResourceName, mApplyingResourcePackPath, exception.ToString());
                    }

                    mFailureFlag = true;
                    return false;
                }
                finally
                {
                    mApplyingResourcePackStream.Position = position;
                }
            }

            private bool DownloadResource(UpdateInfo updateInfo)
            {
                if (updateInfo.Downloading)
                {
                    return false;
                }

                updateInfo.Downloading = true;
                string resourceFullNameWithCrc32 = updateInfo.ResourceName.Variant != null ? Utility.Text.Format("{0}.{1}.{2:x8}.{3}", updateInfo.ResourceName.Name, updateInfo.ResourceName.Variant, updateInfo.HashCode, DefaultExtension) : Utility.Text.Format("{0}.{1:x8}.{2}", updateInfo.ResourceName.Name, updateInfo.HashCode, DefaultExtension);
                mDownloadManager.AddDownload(updateInfo.ResourcePath, Utility.Path.GetRemotePath(Path.Combine(mResourceManager.mUpdatePrefixUri, resourceFullNameWithCrc32)), updateInfo);
                return true;
            }

            private void GenerateReadWriteVersionList()
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(mReadWriteVersionListTempFileName, FileMode.Create, FileAccess.Write);
                    LocalVersionList.Resource[] resources = mResourceManager.mReadWriteResourceInfos.Count > 0 ? new LocalVersionList.Resource[mResourceManager.mReadWriteResourceInfos.Count] : null;
                    if (resources != null)
                    {
                        int index = 0;
                        foreach (KeyValuePair<ResourceName, ReadWriteResourceInfo> i in mResourceManager.mReadWriteResourceInfos)
                        {
                            ResourceName resourceName = i.Key;
                            ReadWriteResourceInfo resourceInfo = i.Value;
                            resources[index] = new LocalVersionList.Resource(resourceName.Name, resourceName.Variant, resourceName.Extension, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                            if (resourceInfo.UseFileSystem)
                            {
                                List<int> resourceIndexes = null;
                                if (!mCachedFileSystemsForGenerateReadWriteVersionList.TryGetValue(resourceInfo.FileSystemName, out resourceIndexes))
                                {
                                    resourceIndexes = new List<int>();
                                    mCachedFileSystemsForGenerateReadWriteVersionList.Add(resourceInfo.FileSystemName, resourceIndexes);
                                }

                                resourceIndexes.Add(index);
                            }

                            index++;
                        }
                    }

                    LocalVersionList.FileSystem[] fileSystems = mCachedFileSystemsForGenerateReadWriteVersionList.Count > 0 ? new LocalVersionList.FileSystem[mCachedFileSystemsForGenerateReadWriteVersionList.Count] : null;
                    if (fileSystems != null)
                    {
                        int index = 0;
                        foreach (KeyValuePair<string, List<int>> i in mCachedFileSystemsForGenerateReadWriteVersionList)
                        {
                            fileSystems[index++] = new LocalVersionList.FileSystem(i.Key, i.Value.ToArray());
                            i.Value.Clear();
                        }
                    }

                    LocalVersionList versionList = new LocalVersionList(resources, fileSystems);
                    if (!mResourceManager.mReadWriteVersionListSerializer.Serialize(fileStream, versionList))
                    {
                        throw new GameFrameworkException("Serialize read-write version list failure.");
                    }

                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }
                catch (Exception exception)
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }

                    if (File.Exists(mReadWriteVersionListTempFileName))
                    {
                        File.Delete(mReadWriteVersionListTempFileName);
                    }

                    throw new GameFrameworkException(Utility.Text.Format("Generate read-write version list exception '{0}'.", exception), exception);
                }

                if (File.Exists(mReadWriteVersionListFileName))
                {
                    File.Delete(mReadWriteVersionListFileName);
                }

                File.Move(mReadWriteVersionListTempFileName, mReadWriteVersionListFileName);
                mCurrentGenerateReadWriteVersionListLength = 0;
            }

            private void OnDownloadStart(object sender, DownloadStartEventArgs e)
            {
                UpdateInfo updateInfo = e.UserData as UpdateInfo;
                if (updateInfo == null)
                {
                    return;
                }

                if (mDownloadManager == null)
                {
                    throw new GameFrameworkException("You must set download manager first.");
                }

                if (e.CurrentLength > int.MaxValue)
                {
                    throw new GameFrameworkException(Utility.Text.Format("File '{0}' is too large.", e.DownloadPath));
                }

                if (ResourceUpdateStart != null)
                {
                    ResourceUpdateStart(updateInfo.ResourceName, e.DownloadPath, e.DownloadUri, (int)e.CurrentLength, updateInfo.CompressedLength, updateInfo.RetryCount);
                }
            }

            private void OnDownloadUpdate(object sender, DownloadUpdateEventArgs e)
            {
                UpdateInfo updateInfo = e.UserData as UpdateInfo;
                if (updateInfo == null)
                {
                    return;
                }

                if (mDownloadManager == null)
                {
                    throw new GameFrameworkException("You must set download manager first.");
                }

                if (e.CurrentLength > updateInfo.CompressedLength)
                {
                    mDownloadManager.RemoveDownload(e.SerialId);
                    string downloadFile = Utility.Text.Format("{0}.download", e.DownloadPath);
                    if (File.Exists(downloadFile))
                    {
                        File.Delete(downloadFile);
                    }

                    string errorMessage = Utility.Text.Format("When download update, downloaded length is larger than compressed length, need '{0}', downloaded '{1}'.", updateInfo.CompressedLength, e.CurrentLength);
                    DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                    OnDownloadFailure(this, downloadFailureEventArgs);
                    ReferencePool.Release(downloadFailureEventArgs);
                    return;
                }

                if (ResourceUpdateChanged != null)
                {
                    ResourceUpdateChanged(updateInfo.ResourceName, e.DownloadPath, e.DownloadUri, (int)e.CurrentLength, updateInfo.CompressedLength);
                }
            }

            private void OnDownloadSuccess(object sender, DownloadSuccessEventArgs e)
            {
                UpdateInfo updateInfo = e.UserData as UpdateInfo;
                if (updateInfo == null)
                {
                    return;
                }

                try
                {
                    using (FileStream fileStream = new FileStream(e.DownloadPath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        bool compressed = updateInfo.Length != updateInfo.CompressedLength || updateInfo.HashCode != updateInfo.CompressedHashCode;

                        int length = (int)fileStream.Length;
                        if (length != updateInfo.CompressedLength)
                        {
                            fileStream.Close();
                            string errorMessage = Utility.Text.Format("Resource compressed length error, need '{0}', downloaded '{1}'.", updateInfo.CompressedLength, length);
                            DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                            OnDownloadFailure(this, downloadFailureEventArgs);
                            ReferencePool.Release(downloadFailureEventArgs);
                            return;
                        }

                        if (compressed)
                        {
                            fileStream.Position = 0L;
                            int hashCode = Utility.Verifier.GetCrc32(fileStream);
                            if (hashCode != updateInfo.CompressedHashCode)
                            {
                                fileStream.Close();
                                string errorMessage = Utility.Text.Format("Resource compressed hash code error, need '{0}', downloaded '{1}'.", updateInfo.CompressedHashCode, hashCode);
                                DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                                OnDownloadFailure(this, downloadFailureEventArgs);
                                ReferencePool.Release(downloadFailureEventArgs);
                                return;
                            }

                            fileStream.Position = 0L;
                            mResourceManager.PrepareCachedStream();
                            if (!Utility.Compression.Decompress(fileStream, mResourceManager.mCachedStream))
                            {
                                fileStream.Close();
                                string errorMessage = Utility.Text.Format("Unable to decompress resource '{0}'.", e.DownloadPath);
                                DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                                OnDownloadFailure(this, downloadFailureEventArgs);
                                ReferencePool.Release(downloadFailureEventArgs);
                                return;
                            }

                            int uncompressedLength = (int)mResourceManager.mCachedStream.Length;
                            if (uncompressedLength != updateInfo.Length)
                            {
                                fileStream.Close();
                                string errorMessage = Utility.Text.Format("Resource length error, need '{0}', downloaded '{1}'.", updateInfo.Length, uncompressedLength);
                                DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                                OnDownloadFailure(this, downloadFailureEventArgs);
                                ReferencePool.Release(downloadFailureEventArgs);
                                return;
                            }

                            fileStream.Position = 0L;
                            fileStream.SetLength(0L);
                            fileStream.Write(mResourceManager.mCachedStream.GetBuffer(), 0, uncompressedLength);
                        }
                        else
                        {
                            int hashCode = 0;
                            fileStream.Position = 0L;
                            if (updateInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || updateInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt
                                || updateInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || updateInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                            {
                                Utility.Converter.GetBytes(updateInfo.HashCode, mCachedHashBytes);
                                if (updateInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || updateInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt)
                                {
                                    hashCode = Utility.Verifier.GetCrc32(fileStream, mCachedHashBytes, Utility.Encryption.QuickEncryptLength);
                                }
                                else if (updateInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt || updateInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                                {
                                    hashCode = Utility.Verifier.GetCrc32(fileStream, mCachedHashBytes, length);
                                }

                                Array.Clear(mCachedHashBytes, 0, CachedHashBytesLength);
                            }
                            else
                            {
                                hashCode = Utility.Verifier.GetCrc32(fileStream);
                            }

                            if (hashCode != updateInfo.HashCode)
                            {
                                fileStream.Close();
                                string errorMessage = Utility.Text.Format("Resource hash code error, need '{0}', downloaded '{1}'.", updateInfo.HashCode, hashCode);
                                DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                                OnDownloadFailure(this, downloadFailureEventArgs);
                                ReferencePool.Release(downloadFailureEventArgs);
                                return;
                            }
                        }
                    }

                    if (updateInfo.UseFileSystem)
                    {
                        IFileSystem fileSystem = mResourceManager.GetFileSystem(updateInfo.FileSystemName, false);
                        bool retVal = fileSystem.WriteFile(updateInfo.ResourceName.FullName, updateInfo.ResourcePath);
                        if (File.Exists(updateInfo.ResourcePath))
                        {
                            File.Delete(updateInfo.ResourcePath);
                        }

                        if (!retVal)
                        {
                            string errorMessage = Utility.Text.Format("Write resource to file system '{0}' error.", fileSystem.FullPath);
                            DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                            OnDownloadFailure(this, downloadFailureEventArgs);
                            ReferencePool.Release(downloadFailureEventArgs);
                            return;
                        }
                    }

                    mUpdateCandidateInfo.Remove(updateInfo.ResourceName);
                    mUpdateWaitingInfo.Remove(updateInfo);
                    mUpdateWaitingInfoWhilePlaying.Remove(updateInfo);
                    mResourceManager.mResourceInfos[updateInfo.ResourceName].MarkReady();
                    mResourceManager.mReadWriteResourceInfos.Add(updateInfo.ResourceName, new ReadWriteResourceInfo(updateInfo.FileSystemName, updateInfo.LoadType, updateInfo.Length, updateInfo.HashCode));
                    if (ResourceUpdateSuccess != null)
                    {
                        ResourceUpdateSuccess(updateInfo.ResourceName, e.DownloadPath, e.DownloadUri, updateInfo.Length, updateInfo.CompressedLength);
                    }

                    mCurrentGenerateReadWriteVersionListLength += updateInfo.CompressedLength;
                    if (mUpdateCandidateInfo.Count <= 0 || mUpdateWaitingInfo.Count + mUpdateWaitingInfoWhilePlaying.Count <= 0 || mCurrentGenerateReadWriteVersionListLength >= mGenerateReadWriteVersionListLength)
                    {
                        GenerateReadWriteVersionList();
                    }

                    if (mUpdatingResourceGroup != null && mUpdateWaitingInfo.Count <= 0)
                    {
                        ResourceGroup updatingResourceGroup = mUpdatingResourceGroup;
                        mUpdatingResourceGroup = null;
                        if (ResourceUpdateComplete != null)
                        {
                            ResourceUpdateComplete(updatingResourceGroup, !mFailureFlag);
                        }
                    }

                    if (mUpdateCandidateInfo.Count <= 0 && ResourceUpdateAllComplete != null)
                    {
                        ResourceUpdateAllComplete();
                    }
                }
                catch (Exception exception)
                {
                    string errorMessage = Utility.Text.Format("Update resource '{0}' with error message '{1}'.", e.DownloadPath, exception);
                    DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                    OnDownloadFailure(this, downloadFailureEventArgs);
                    ReferencePool.Release(downloadFailureEventArgs);
                }
            }

            private void OnDownloadFailure(object sender, DownloadFailureEventArgs e)
            {
                UpdateInfo updateInfo = e.UserData as UpdateInfo;
                if (updateInfo == null)
                {
                    return;
                }

                if (File.Exists(e.DownloadPath))
                {
                    File.Delete(e.DownloadPath);
                }

                if (ResourceUpdateFailure != null)
                {
                    ResourceUpdateFailure(updateInfo.ResourceName, e.DownloadUri, updateInfo.RetryCount, mUpdateRetryCount, e.ErrorMessage);
                }

                if (updateInfo.RetryCount < mUpdateRetryCount)
                {
                    updateInfo.Downloading = false;
                    updateInfo.RetryCount++;
                    if (mUpdateWaitingInfoWhilePlaying.Contains(updateInfo))
                    {
                        DownloadResource(updateInfo);
                    }
                }
                else
                {
                    mFailureFlag = true;
                    updateInfo.Downloading = false;
                    updateInfo.RetryCount = 0;
                    mUpdateWaitingInfo.Remove(updateInfo);
                    mUpdateWaitingInfoWhilePlaying.Remove(updateInfo);
                }
            }
        }
    }
}
