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
        private sealed partial class ResourceChecker
        {
            private sealed partial class CheckInfo
            {
                [StructLayout(LayoutKind.Auto)]
                private struct RemoteVersionInfo
                {
                    private readonly bool mExist;
                    private readonly string mFileSystemName;
                    private readonly LoadType mLoadType;
                    private readonly int mLength;
                    private readonly int mHashCode;
                    private readonly int mCompressedLength;
                    private readonly int mCompressedHashCode;

                    public RemoteVersionInfo(string fileSystemName, LoadType loadType, int length, int hashCode, int compressedLength, int compressedHashCode)
                    {
                        mExist = true;
                        mFileSystemName = fileSystemName;
                        mLoadType = loadType;
                        mLength = length;
                        mHashCode = hashCode;
                        mCompressedLength = compressedLength;
                        mCompressedHashCode = compressedHashCode;
                    }

                    public bool Exist
                    {
                        get
                        {
                            return mExist;
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
                }
            }
        }
    }
}
