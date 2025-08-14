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
    public partial struct LocalVersionList
    {
        [StructLayout(LayoutKind.Auto)]
        public struct Resource
        {
            private readonly string mName;
            private readonly string mVariant;
            private readonly string mExtension;
            private readonly byte mLoadType;
            private readonly int mLength;
            private readonly int mHashCode;

            public Resource(string name, string variant, string extension, byte loadType, int length, int hashCode)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                mName = name;
                mVariant = variant;
                mExtension = extension;
                mLoadType = loadType;
                mLength = length;
                mHashCode = hashCode;
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
        }
    }
}
