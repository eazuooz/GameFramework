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
    public partial struct ResourcePackVersionList
    {
        [StructLayout(LayoutKind.Auto)]
        public struct Resource
        {
            private readonly string mName;
            private readonly string mVariant;
            private readonly string mExtension;
            private readonly byte mLoadType;
            private readonly long mOffset;
            private readonly int mLength;
            private readonly int mHashCode;
            private readonly int mCompressedLength;
            private readonly int mCompressedHashCode;

            public Resource(string name, string variant, string extension, byte loadType, long offset, int length, int hashCode, int compressedLength, int compressedHashCode)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                mName = name;
                mVariant = variant;
                mExtension = extension;
                mLoadType = loadType;
                mOffset = offset;
                mLength = length;
                mHashCode = hashCode;
                mCompressedLength = compressedLength;
                mCompressedHashCode = compressedHashCode;
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public string Variant
            {
                get
                {
                    return mVariant;
                }
            }

            public string Extension
            {
                get
                {
                    return mExtension;
                }
            }

            public byte LoadType
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
        }
    }
}
