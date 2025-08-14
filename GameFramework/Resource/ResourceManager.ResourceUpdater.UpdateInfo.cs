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
        private sealed partial class ResourceUpdater
        {
            private sealed class UpdateInfo
            {
                private readonly ResourceName mResourceName;
                private readonly string mFileSystemName;
                private readonly LoadType mLoadType;
                private readonly int mLength;
                private readonly int mHashCode;
                private readonly int mCompressedLength;
                private readonly int mCompressedHashCode;
                private readonly string mResourcePath;
                private bool mDownloading;
                private int mRetryCount;

                public UpdateInfo(ResourceName resourceName, string fileSystemName, LoadType loadType, int length, int hashCode, int compressedLength, int compressedHashCode, string resourcePath)
                {
                    mResourceName = resourceName;
                    mFileSystemName = fileSystemName;
                    mLoadType = loadType;
                    mLength = length;
                    mHashCode = hashCode;
                    mCompressedLength = compressedLength;
                    mCompressedHashCode = compressedHashCode;
                    mResourcePath = resourcePath;
                    mDownloading = false;
                    mRetryCount = 0;
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

                public int CompressedHashCode
                {
                    get
                    {
                        return mCompressedHashCode;
                    }
                }

                public string ResourcePath
                {
                    get
                    {
                        return mResourcePath;
                    }
                }

                public bool Downloading
                {
                    get
                    {
                        return mDownloading;
                    }
                    set
                    {
                        mDownloading = value;
                    }
                }

                public int RetryCount
                {
                    get
                    {
                        return mRetryCount;
                    }
                    set
                    {
                        mRetryCount = value;
                    }
                }
            }
        }
    }
}
