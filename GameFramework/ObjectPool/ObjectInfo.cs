//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace GameFramework.ObjectPool
{
    [StructLayout(LayoutKind.Auto)]
    public struct ObjectInfo
    {
        private readonly string mName;
        private readonly bool mLocked;
        private readonly bool mCustomCanReleaseFlag;
        private readonly int mPriority;
        private readonly DateTime mLastUseTime;
        private readonly int mSpawnCount;

        public ObjectInfo(string name, bool locked, bool customCanReleaseFlag, int priority, DateTime lastUseTime, int spawnCount)
        {
            mName = name;
            mLocked = locked;
            mCustomCanReleaseFlag = customCanReleaseFlag;
            mPriority = priority;
            mLastUseTime = lastUseTime;
            mSpawnCount = spawnCount;
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public bool Locked
        {
            get
            {
                return mLocked;
            }
        }

        public bool CustomCanReleaseFlag
        {
            get
            {
                return mCustomCanReleaseFlag;
            }
        }

        public int Priority
        {
            get
            {
                return mPriority;
            }
        }

        public DateTime LastUseTime
        {
            get
            {
                return mLastUseTime;
            }
        }

        public bool IsInUse
        {
            get
            {
                return mSpawnCount > 0;
            }
        }

        public int SpawnCount
        {
            get
            {
                return mSpawnCount;
            }
        }
    }
}
