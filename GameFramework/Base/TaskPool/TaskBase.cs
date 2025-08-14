//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    internal abstract class TaskBase : IReference
    {
        public TaskBase()
        {
            mSerialId = 0;
            mTag = null;
            mPriority = DefaultPriority;
            mDone = false;
            mUserData = null;
        }

        internal void Initialize(int serialId, string tag, int priority, object userData)
        {
            mSerialId = serialId;
            mTag = tag;
            mPriority = priority;
            mUserData = userData;
            mDone = false;
        }

        public virtual void Clear()
        {
            mSerialId = 0;
            mTag = null;
            mPriority = DefaultPriority;
            mUserData = null;
            mDone = false;
        }

        public int SerialId
        {
            get
            {
                return mSerialId;
            }
        }

        public string Tag
        {
            get
            {
                return mTag;
            }
        }

        public int Priority
        {
            get
            {
                return mPriority;
            }
        }

        public object UserData
        {
            get
            {
                return mUserData;
            }
        }

        public bool Done
        {
            get
            {
                return mDone;
            }
            set
            {
                mDone = value;
            }
        }

        public virtual string Description
        {
            get
            {
                return null;
            }
        }

        public const int DefaultPriority = 0;

        private int mSerialId;
        private string mTag;
        private int mPriority;
        private object mUserData;

        private bool mDone;
    }
}