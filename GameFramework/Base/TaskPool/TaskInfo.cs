//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameFramework
{
    [StructLayout(LayoutKind.Auto)]
    public struct TaskInfo
    {
        public TaskInfo(int serialId, string tag, int priority, object userData, TaskStatus status, string description)
        {
            mIsValid = true;
            mSerialId = serialId;
            mTag = tag;
            mPriority = priority;
            mUserData = userData;
            mStatus = status;
            mDescription = description;
        }

        public bool IsValid
        {
            get
            {
                return mIsValid;
            }
        }

        public int SerialId
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mSerialId;
            }
        }

        public string Tag
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mTag;
            }
        }

        public int Priority
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mPriority;
            }
        }

        public object UserData
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mUserData;
            }
        }

        public TaskStatus Status
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mStatus;
            }
        }

        public string Description
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mDescription;
            }
        }

        private readonly bool mIsValid;
        private readonly int mSerialId;
        private readonly string mTag;
        private readonly int mPriority;
        private readonly object mUserData;
        private readonly TaskStatus mStatus;
        private readonly string mDescription;
    }
}