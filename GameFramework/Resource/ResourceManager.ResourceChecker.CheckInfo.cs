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
        private sealed partial class ResourceChecker
        {
            private sealed partial class CheckInfo
            {
                private readonly ResourceName mResourceName;
                private CheckStatus mStatus;
                private bool mNeedRemove;
                private bool mNeedMoveToDisk;
                private bool mNeedMoveToFileSystem;
                private RemoteVersionInfo mVersionInfo;
                private LocalVersionInfo mReadOnlyInfo;
                private LocalVersionInfo mReadWriteInfo;
                private string mCachedFileSystemName;

                public CheckInfo(ResourceName resourceName)
                {
                    mResourceName = resourceName;
                    mStatus = CheckStatus.Unknown;
                    mNeedRemove = false;
                    mNeedMoveToDisk = false;
                    mNeedMoveToFileSystem = false;
                    mVersionInfo = default(RemoteVersionInfo);
                    mReadOnlyInfo = default(LocalVersionInfo);
                    mReadWriteInfo = default(LocalVersionInfo);
                    mCachedFileSystemName = null;
                }

                public ResourceName ResourceName
                {
                    get
                    {
                        return mResourceName;
                    }
                }

                public CheckStatus Status
                {
                    get
                    {
                        return mStatus;
                    }
                }

                public bool NeedRemove
                {
                    get
                    {
                        return mNeedRemove;
                    }
                }

                public bool NeedMoveToDisk
                {
                    get
                    {
                        return mNeedMoveToDisk;
                    }
                }

                public bool NeedMoveToFileSystem
                {
                    get
                    {
                        return mNeedMoveToFileSystem;
                    }
                }

                public string FileSystemName
                {
                    get
                    {
                        return mVersionInfo.FileSystemName;
                    }
                }

                public bool ReadWriteUseFileSystem
                {
                    get
                    {
                        return mReadWriteInfo.UseFileSystem;
                    }
                }

                public string ReadWriteFileSystemName
                {
                    get
                    {
                        return mReadWriteInfo.FileSystemName;
                    }
                }

                public LoadType LoadType
                {
                    get
                    {
                        return mVersionInfo.LoadType;
                    }
                }

                public int Length
                {
                    get
                    {
                        return mVersionInfo.Length;
                    }
                }

                public int HashCode
                {
                    get
                    {
                        return mVersionInfo.HashCode;
                    }
                }

                public int CompressedLength
                {
                    get
                    {
                        return mVersionInfo.CompressedLength;
                    }
                }

                public int CompressedHashCode
                {
                    get
                    {
                        return mVersionInfo.CompressedHashCode;
                    }
                }

                public void SetCachedFileSystemName(string fileSystemName)
                {
                    mCachedFileSystemName = fileSystemName;
                }

                public void SetVersionInfo(LoadType loadType, int length, int hashCode, int compressedLength, int compressedHashCode)
                {
                    if (mVersionInfo.Exist)
                    {
                        throw new GameFrameworkException(Utility.Text.Format("You must set version info of '{0}' only once.", mResourceName.FullName));
                    }

                    mVersionInfo = new RemoteVersionInfo(mCachedFileSystemName, loadType, length, hashCode, compressedLength, compressedHashCode);
                    mCachedFileSystemName = null;
                }

                public void SetReadOnlyInfo(LoadType loadType, int length, int hashCode)
                {
                    if (mReadOnlyInfo.Exist)
                    {
                        throw new GameFrameworkException(Utility.Text.Format("You must set read-only info of '{0}' only once.", mResourceName.FullName));
                    }

                    mReadOnlyInfo = new LocalVersionInfo(mCachedFileSystemName, loadType, length, hashCode);
                    mCachedFileSystemName = null;
                }

                public void SetReadWriteInfo(LoadType loadType, int length, int hashCode)
                {
                    if (mReadWriteInfo.Exist)
                    {
                        throw new GameFrameworkException(Utility.Text.Format("You must set read-write info of '{0}' only once.", mResourceName.FullName));
                    }

                    mReadWriteInfo = new LocalVersionInfo(mCachedFileSystemName, loadType, length, hashCode);
                    mCachedFileSystemName = null;
                }

                public void RefreshStatus(string currentVariant, bool ignoreOtherVariant)
                {
                    if (!mVersionInfo.Exist)
                    {
                        mStatus = CheckStatus.Disuse;
                        mNeedRemove = mReadWriteInfo.Exist;
                        return;
                    }

                    if (mResourceName.Variant == null || mResourceName.Variant == currentVariant)
                    {
                        if (mReadOnlyInfo.Exist && mReadOnlyInfo.FileSystemName == mVersionInfo.FileSystemName && mReadOnlyInfo.LoadType == mVersionInfo.LoadType && mReadOnlyInfo.Length == mVersionInfo.Length && mReadOnlyInfo.HashCode == mVersionInfo.HashCode)
                        {
                            mStatus = CheckStatus.StorageInReadOnly;
                            mNeedRemove = mReadWriteInfo.Exist;
                        }
                        else if (mReadWriteInfo.Exist && mReadWriteInfo.LoadType == mVersionInfo.LoadType && mReadWriteInfo.Length == mVersionInfo.Length && mReadWriteInfo.HashCode == mVersionInfo.HashCode)
                        {
                            bool differentFileSystem = mReadWriteInfo.FileSystemName != mVersionInfo.FileSystemName;
                            mStatus = CheckStatus.StorageInReadWrite;
                            mNeedMoveToDisk = mReadWriteInfo.UseFileSystem && differentFileSystem;
                            mNeedMoveToFileSystem = mVersionInfo.UseFileSystem && differentFileSystem;
                        }
                        else
                        {
                            mStatus = CheckStatus.Update;
                            mNeedRemove = mReadWriteInfo.Exist;
                        }
                    }
                    else
                    {
                        mStatus = CheckStatus.Unavailable;
                        mNeedRemove = !ignoreOtherVariant && mReadWriteInfo.Exist;
                    }
                }
            }
        }
    }
}
