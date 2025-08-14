//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameFramework.FileSystem
{
    internal sealed partial class FileSystem : IFileSystem
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct HeaderData
        {
            private const int HeaderLength = 3;
            private const int FileSystemVersion = 0;
            private const int EncryptBytesLength = 4;
            private static readonly byte[] Header = new byte[HeaderLength] { (byte)'G', (byte)'F', (byte)'F' };

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HeaderLength)]
            private readonly byte[] mHeader;

            private readonly byte mVersion;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = EncryptBytesLength)]
            private readonly byte[] mEncryptBytes;

            private readonly int mMaxFileCount;
            private readonly int mMaxBlockCount;
            private readonly int mBlockCount;

            public HeaderData(int maxFileCount, int maxBlockCount)
                : this(FileSystemVersion, new byte[EncryptBytesLength], maxFileCount, maxBlockCount, 0)
            {
                Utility.Random.GetRandomBytes(mEncryptBytes);
            }

            public HeaderData(byte version, byte[] encryptBytes, int maxFileCount, int maxBlockCount, int blockCount)
            {
                mHeader = Header;
                mVersion = version;
                mEncryptBytes = encryptBytes;
                mMaxFileCount = maxFileCount;
                mMaxBlockCount = maxBlockCount;
                mBlockCount = blockCount;
            }

            public bool IsValid
            {
                get
                {
                    return mHeader.Length == HeaderLength && mHeader[0] == Header[0] && mHeader[1] == Header[1] && mHeader[2] == Header[2] && mVersion == FileSystemVersion && mEncryptBytes.Length == EncryptBytesLength
                        && mMaxFileCount > 0 && mMaxBlockCount > 0 && mMaxFileCount <= mMaxBlockCount && mBlockCount > 0 && mBlockCount <= mMaxBlockCount;
                }
            }

            public byte Version
            {
                get
                {
                    return mVersion;
                }
            }

            public int MaxFileCount
            {
                get
                {
                    return mMaxFileCount;
                }
            }

            public int MaxBlockCount
            {
                get
                {
                    return mMaxBlockCount;
                }
            }

            public int BlockCount
            {
                get
                {
                    return mBlockCount;
                }
            }

            public byte[] GetEncryptBytes()
            {
                return mEncryptBytes;
            }

            public HeaderData SetBlockCount(int blockCount)
            {
                return new HeaderData(mVersion, mEncryptBytes, mMaxFileCount, mMaxBlockCount, blockCount);
            }
        }
    }
}
