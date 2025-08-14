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
        private sealed partial class ResourceVerifier
        {
            private const int CachedHashBytesLength = 4;

            private readonly ResourceManager mResourceManager;
            private readonly List<VerifyInfo> mVerifyInfos;
            private readonly byte[] mCachedHashBytes;
            private bool mLoadReadWriteVersionListComplete;
            private int mVerifyResourceLengthPerFrame;
            private int mVerifyResourceIndex;
            private bool mFailureFlag;

            public GameFrameworkAction<int, long> ResourceVerifyStart;
            public GameFrameworkAction<ResourceName, int> ResourceVerifySuccess;
            public GameFrameworkAction<ResourceName> ResourceVerifyFailure;
            public GameFrameworkAction<bool> ResourceVerifyComplete;

            public ResourceVerifier(ResourceManager resourceManager)
            {
                mResourceManager = resourceManager;
                mVerifyInfos = new List<VerifyInfo>();
                mCachedHashBytes = new byte[CachedHashBytesLength];
                mLoadReadWriteVersionListComplete = false;
                mVerifyResourceLengthPerFrame = 0;
                mVerifyResourceIndex = 0;
                mFailureFlag = false;

                ResourceVerifyStart = null;
                ResourceVerifySuccess = null;
                ResourceVerifyFailure = null;
                ResourceVerifyComplete = null;
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (!mLoadReadWriteVersionListComplete)
                {
                    return;
                }

                int length = 0;
                while (mVerifyResourceIndex < mVerifyInfos.Count)
                {
                    VerifyInfo verifyInfo = mVerifyInfos[mVerifyResourceIndex];
                    length += verifyInfo.Length;
                    if (VerifyResource(verifyInfo))
                    {
                        mVerifyResourceIndex++;
                        if (ResourceVerifySuccess != null)
                        {
                            ResourceVerifySuccess(verifyInfo.ResourceName, verifyInfo.Length);
                        }
                    }
                    else
                    {
                        mFailureFlag = true;
                        mVerifyInfos.RemoveAt(mVerifyResourceIndex);
                        if (ResourceVerifyFailure != null)
                        {
                            ResourceVerifyFailure(verifyInfo.ResourceName);
                        }
                    }

                    if (length >= mVerifyResourceLengthPerFrame)
                    {
                        return;
                    }
                }

                mLoadReadWriteVersionListComplete = false;
                if (mFailureFlag)
                {
                    GenerateReadWriteVersionList();
                }

                if (ResourceVerifyComplete != null)
                {
                    ResourceVerifyComplete(!mFailureFlag);
                }
            }

            public void Shutdown()
            {
                mVerifyInfos.Clear();
                mLoadReadWriteVersionListComplete = false;
                mVerifyResourceLengthPerFrame = 0;
                mVerifyResourceIndex = 0;
                mFailureFlag = false;
            }

            public void VerifyResources(int verifyResourceLengthPerFrame)
            {
                if (verifyResourceLengthPerFrame < 0)
                {
                    throw new GameFrameworkException("Verify resource count per frame is invalid.");
                }

                if (mResourceManager.mResourceHelper == null)
                {
                    throw new GameFrameworkException("Resource helper is invalid.");
                }

                if (string.IsNullOrEmpty(mResourceManager.mReadWritePath))
                {
                    throw new GameFrameworkException("Read-write path is invalid.");
                }

                mVerifyResourceLengthPerFrame = verifyResourceLengthPerFrame;
                mResourceManager.mResourceHelper.LoadBytes(Utility.Path.GetRemotePath(Path.Combine(mResourceManager.mReadWritePath, LocalVersionListFileName)), new LoadBytesCallbacks(OnLoadReadWriteVersionListSuccess, OnLoadReadWriteVersionListFailure), null);
            }

            private bool VerifyResource(VerifyInfo verifyInfo)
            {
                if (verifyInfo.UseFileSystem)
                {
                    IFileSystem fileSystem = mResourceManager.GetFileSystem(verifyInfo.FileSystemName, false);
                    string fileName = verifyInfo.ResourceName.FullName;
                    FileSystem.FileInfo fileInfo = fileSystem.GetFileInfo(fileName);
                    if (!fileInfo.IsValid)
                    {
                        return false;
                    }

                    int length = fileInfo.Length;
                    if (length == verifyInfo.Length)
                    {
                        mResourceManager.PrepareCachedStream();
                        fileSystem.ReadFile(fileName, mResourceManager.mCachedStream);
                        mResourceManager.mCachedStream.Position = 0L;
                        int hashCode = 0;
                        if (verifyInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || verifyInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt
                            || verifyInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || verifyInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                        {
                            Utility.Converter.GetBytes(verifyInfo.HashCode, mCachedHashBytes);
                            if (verifyInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || verifyInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt)
                            {
                                hashCode = Utility.Verifier.GetCrc32(mResourceManager.mCachedStream, mCachedHashBytes, Utility.Encryption.QuickEncryptLength);
                            }
                            else if (verifyInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt || verifyInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                            {
                                hashCode = Utility.Verifier.GetCrc32(mResourceManager.mCachedStream, mCachedHashBytes, length);
                            }

                            Array.Clear(mCachedHashBytes, 0, CachedHashBytesLength);
                        }
                        else
                        {
                            hashCode = Utility.Verifier.GetCrc32(mResourceManager.mCachedStream);
                        }

                        if (hashCode == verifyInfo.HashCode)
                        {
                            return true;
                        }
                    }

                    fileSystem.DeleteFile(fileName);
                    return false;
                }
                else
                {
                    string resourcePath = Utility.Path.GetRegularPath(Path.Combine(mResourceManager.ReadWritePath, verifyInfo.ResourceName.FullName));
                    if (!File.Exists(resourcePath))
                    {
                        return false;
                    }

                    using (FileStream fileStream = new FileStream(resourcePath, FileMode.Open, FileAccess.Read))
                    {
                        int length = (int)fileStream.Length;
                        if (length == verifyInfo.Length)
                        {
                            int hashCode = 0;
                            if (verifyInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || verifyInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt
                                || verifyInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt || verifyInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                            {
                                Utility.Converter.GetBytes(verifyInfo.HashCode, mCachedHashBytes);
                                if (verifyInfo.LoadType == LoadType.LoadFromMemoryAndQuickDecrypt || verifyInfo.LoadType == LoadType.LoadFromBinaryAndQuickDecrypt)
                                {
                                    hashCode = Utility.Verifier.GetCrc32(fileStream, mCachedHashBytes, Utility.Encryption.QuickEncryptLength);
                                }
                                else if (verifyInfo.LoadType == LoadType.LoadFromMemoryAndDecrypt || verifyInfo.LoadType == LoadType.LoadFromBinaryAndDecrypt)
                                {
                                    hashCode = Utility.Verifier.GetCrc32(fileStream, mCachedHashBytes, length);
                                }

                                Array.Clear(mCachedHashBytes, 0, CachedHashBytesLength);
                            }
                            else
                            {
                                hashCode = Utility.Verifier.GetCrc32(fileStream);
                            }

                            if (hashCode == verifyInfo.HashCode)
                            {
                                return true;
                            }
                        }
                    }

                    File.Delete(resourcePath);
                    return false;
                }
            }

            private void GenerateReadWriteVersionList()
            {
                string readWriteVersionListFileName = Utility.Path.GetRegularPath(Path.Combine(mResourceManager.mReadWritePath, LocalVersionListFileName));
                string readWriteVersionListTempFileName = Utility.Text.Format("{0}.{1}", readWriteVersionListFileName, TempExtension);
                SortedDictionary<string, List<int>> cachedFileSystemsForGenerateReadWriteVersionList = new SortedDictionary<string, List<int>>(StringComparer.Ordinal);
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(readWriteVersionListTempFileName, FileMode.Create, FileAccess.Write);
                    LocalVersionList.Resource[] resources = mVerifyInfos.Count > 0 ? new LocalVersionList.Resource[mVerifyInfos.Count] : null;
                    if (resources != null)
                    {
                        int index = 0;
                        foreach (VerifyInfo i in mVerifyInfos)
                        {
                            resources[index] = new LocalVersionList.Resource(i.ResourceName.Name, i.ResourceName.Variant, i.ResourceName.Extension, (byte)i.LoadType, i.Length, i.HashCode);
                            if (i.UseFileSystem)
                            {
                                List<int> resourceIndexes = null;
                                if (!cachedFileSystemsForGenerateReadWriteVersionList.TryGetValue(i.FileSystemName, out resourceIndexes))
                                {
                                    resourceIndexes = new List<int>();
                                    cachedFileSystemsForGenerateReadWriteVersionList.Add(i.FileSystemName, resourceIndexes);
                                }

                                resourceIndexes.Add(index);
                            }

                            index++;
                        }
                    }

                    LocalVersionList.FileSystem[] fileSystems = cachedFileSystemsForGenerateReadWriteVersionList.Count > 0 ? new LocalVersionList.FileSystem[cachedFileSystemsForGenerateReadWriteVersionList.Count] : null;
                    if (fileSystems != null)
                    {
                        int index = 0;
                        foreach (KeyValuePair<string, List<int>> i in cachedFileSystemsForGenerateReadWriteVersionList)
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

                    if (File.Exists(readWriteVersionListTempFileName))
                    {
                        File.Delete(readWriteVersionListTempFileName);
                    }

                    throw new GameFrameworkException(Utility.Text.Format("Generate read-write version list exception '{0}'.", exception), exception);
                }

                if (File.Exists(readWriteVersionListFileName))
                {
                    File.Delete(readWriteVersionListFileName);
                }

                File.Move(readWriteVersionListTempFileName, readWriteVersionListFileName);
            }

            private void OnLoadReadWriteVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
            {
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(bytes, false);
                    LocalVersionList versionList = mResourceManager.mReadWriteVersionListSerializer.Deserialize(memoryStream);
                    if (!versionList.IsValid)
                    {
                        throw new GameFrameworkException("Deserialize read write version list failure.");
                    }

                    LocalVersionList.Resource[] resources = versionList.GetResources();
                    LocalVersionList.FileSystem[] fileSystems = versionList.GetFileSystems();
                    Dictionary<ResourceName, string> resourceInFileSystemNames = new Dictionary<ResourceName, string>();
                    foreach (LocalVersionList.FileSystem fileSystem in fileSystems)
                    {
                        int[] resourceIndexes = fileSystem.GetResourceIndexes();
                        foreach (int resourceIndex in resourceIndexes)
                        {
                            LocalVersionList.Resource resource = resources[resourceIndex];
                            resourceInFileSystemNames.Add(new ResourceName(resource.Name, resource.Variant, resource.Extension), fileSystem.Name);
                        }
                    }

                    long totalLength = 0L;
                    foreach (LocalVersionList.Resource resource in resources)
                    {
                        ResourceName resourceName = new ResourceName(resource.Name, resource.Variant, resource.Extension);
                        string fileSystemName = null;
                        resourceInFileSystemNames.TryGetValue(resourceName, out fileSystemName);
                        totalLength += resource.Length;
                        mVerifyInfos.Add(new VerifyInfo(resourceName, fileSystemName, (LoadType)resource.LoadType, resource.Length, resource.HashCode));
                    }

                    mLoadReadWriteVersionListComplete = true;
                    if (ResourceVerifyStart != null)
                    {
                        ResourceVerifyStart(mVerifyInfos.Count, totalLength);
                    }
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
                if (ResourceVerifyComplete != null)
                {
                    ResourceVerifyComplete(true);
                }
            }
        }
    }
}
