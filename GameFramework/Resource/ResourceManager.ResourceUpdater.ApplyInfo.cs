//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceUpdater
        {
            [StructLayout(LayoutKind.Auto)]
            private struct ApplyInfo
            {
                private readonly ResourceName mResourceName;
                private readonly string mFileSystemName;
                private readonly LoadType mLoadType;
                private readonly long mOffset;
                private readonly int mLength;
                private readonly int mHashCode;
                private readonly int mCompressedLength;
                private readonly int mCompressedHashCode;
                private readonly string mResourcePath;

                public ApplyInfo(ResourceName resourceName, string fileSystemName, LoadType loadType, long offset, int length, int hashCode, int compressedLength, int compressedHashCode, string resourcePath)
                {
                    mResourceName = resourceName;
                    mFileSystemName = fileSystemName;
                    mLoadType = loadType;
                    mOffset = offset;
                    mLength = length;
                    mHashCode = hashCode;
                    mCompressedLength = compressedLength;
                    mCompressedHashCode = compressedHashCode;
                    mResourcePath = resourcePath;
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

                public long Offset
                {
                    get
                    {
                        return mOffset;
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
            }
        }
    }
}
