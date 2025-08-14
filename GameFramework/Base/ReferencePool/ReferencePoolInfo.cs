//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace GameFramework
{
    [StructLayout(LayoutKind.Auto)]
    public struct ReferencePoolInfo
    {
        private readonly Type mType;
        private readonly int mUnusedReferenceCount;
        private readonly int mUsingReferenceCount;
        private readonly int mAcquireReferenceCount;
        private readonly int mReleaseReferenceCount;
        private readonly int mAddReferenceCount;
        private readonly int mRemoveReferenceCount;

        public ReferencePoolInfo(Type type, int unusedReferenceCount, int usingReferenceCount, int acquireReferenceCount, int releaseReferenceCount, int addReferenceCount, int removeReferenceCount)
        {
            mType = type;
            mUnusedReferenceCount = unusedReferenceCount;
            mUsingReferenceCount = usingReferenceCount;
            mAcquireReferenceCount = acquireReferenceCount;
            mReleaseReferenceCount = releaseReferenceCount;
            mAddReferenceCount = addReferenceCount;
            mRemoveReferenceCount = removeReferenceCount;
        }

        public Type Type
        {
            get
            {
                return mType;
            }
        }

        public int UnusedReferenceCount
        {
            get
            {
                return mUnusedReferenceCount;
            }
        }

        public int UsingReferenceCount
        {
            get
            {
                return mUsingReferenceCount;
            }
        }

        public int AcquireReferenceCount
        {
            get
            {
                return mAcquireReferenceCount;
            }
        }

        public int ReleaseReferenceCount
        {
            get
            {
                return mReleaseReferenceCount;
            }
        }

        public int AddReferenceCount
        {
            get
            {
                return mAddReferenceCount;
            }
        }

        public int RemoveReferenceCount
        {
            get
            {
                return mRemoveReferenceCount;
            }
        }
    }
}