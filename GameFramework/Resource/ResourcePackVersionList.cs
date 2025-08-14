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
    [StructLayout(LayoutKind.Auto)]
    public partial struct ResourcePackVersionList
    {
        private static readonly Resource[] EmptyResourceArray = new Resource[] { };

        private readonly bool mIsValid;
        private readonly int mOffset;
        private readonly long mLength;
        private readonly int mHashCode;
        private readonly Resource[] mResources;

        public ResourcePackVersionList(int offset, long length, int hashCode, Resource[] resources)
        {
            mIsValid = true;
            mOffset = offset;
            mLength = length;
            mHashCode = hashCode;
            mResources = resources ?? EmptyResourceArray;
        }

        public bool IsValid
        {
            get
            {
                return mIsValid;
            }
        }

        public int Offset
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mOffset;
            }
        }

        public long Length
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mLength;
            }
        }

        public int HashCode
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mHashCode;
            }
        }

        public Resource[] GetResources()
        {
            if (!mIsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return mResources;
        }
    }
}
