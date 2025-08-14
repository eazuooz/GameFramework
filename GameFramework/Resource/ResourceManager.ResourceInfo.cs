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
        private sealed class ResourceInfo
        {
            private readonly ResourceName mResourceName;
            private readonly string mFileSystemName;
            private readonly LoadType mLoadType;
            private readonly int mLength;
            private readonly int mHashCode;
            private readonly int mCompressedLength;
            private readonly bool mStorageInReadOnly;
            private bool mReady;

            public ResourceInfo(ResourceName resourceName, string fileSystemName, LoadType loadType, int length, int hashCode, int compressedLength, bool storageInReadOnly, bool ready)
            {
                mResourceName = resourceName;
                mFileSystemName = fileSystemName;
                mLoadType = loadType;
                mLength = length;
                mHashCode = hashCode;
                mCompressedLength = compressedLength;
                mStorageInReadOnly = storageInReadOnly;
                mReady = ready;
            }

            public ResourceName ResourceName
            {
                get
                {
                    return mResourceName;
                }
            }

            public bool UseFileSystem
            {
                get
                {
                    return !string.IsNullOrEmpty(mFileSystemName);
                }
            }

            public string FileSystemName
            {
                get
                {
                    return mFileSystemName;
                }
            }

            public bool IsLoadFromBinary
            {
                get
                {
                    return mLoadType == LoadType.LoadFromBinary || mLoadType == LoadType.LoadFromBinaryAndQuickDecrypt || mLoadType == LoadType.LoadFromBinaryAndDecrypt;
                }
            }

            public LoadType LoadType
            {
                get
                {
                    return mLoadType;
                }
            }

            public int Length
            {
                get
                {
                    return mLength;
                }
            }

            public int HashCode
            {
                get
                {
                    return mHashCode;
                }
            }

            public int CompressedLength
            {
                get
                {
                    return mCompressedLength;
                }
            }

            public bool StorageInReadOnly
            {
                get
                {
                    return mStorageInReadOnly;
                }
            }

            public bool Ready
            {
                get
                {
                    return mReady;
                }
            }

            public void MarkReady()
            {
                mReady = true;
            }
        }
    }
}
