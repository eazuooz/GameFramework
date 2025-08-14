//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.ObjectPool
{
    public abstract class ObjectBase : IReference
    {
        private string mName;
        private object mTarget;
        private bool mLocked;
        private int mPriority;
        private DateTime mLastUseTime;

        public ObjectBase()
        {
            mName = null;
            mTarget = null;
            mLocked = false;
            mPriority = 0;
            mLastUseTime = default(DateTime);
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public object Target
        {
            get
            {
                return mTarget;
            }
        }

        public bool Locked
        {
            get
            {
                return mLocked;
            }
            set
            {
                mLocked = value;
            }
        }

        public int Priority
        {
            get
            {
                return mPriority;
            }
            set
            {
                mPriority = value;
            }
        }

        public virtual bool CustomCanReleaseFlag
        {
            get
            {
                return true;
            }
        }

        public DateTime LastUseTime
        {
            get
            {
                return mLastUseTime;
            }
            internal set
            {
                mLastUseTime = value;
            }
        }

        protected void Initialize(object target)
        {
            Initialize(null, target, false, 0);
        }

        protected void Initialize(string name, object target)
        {
            Initialize(name, target, false, 0);
        }

        protected void Initialize(string name, object target, bool locked)
        {
            Initialize(name, target, locked, 0);
        }

        protected void Initialize(string name, object target, int priority)
        {
            Initialize(name, target, false, priority);
        }

        protected void Initialize(string name, object target, bool locked, int priority)
        {
            if (target == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Target '{0}' is invalid.", name));
            }

            mName = name ?? string.Empty;
            mTarget = target;
            mLocked = locked;
            mPriority = priority;
            mLastUseTime = DateTime.UtcNow;
        }

        public virtual void Clear()
        {
            mName = null;
            mTarget = null;
            mLocked = false;
            mPriority = 0;
            mLastUseTime = default(DateTime);
        }

        protected internal virtual void OnSpawn()
        {
        }

        protected internal virtual void OnUnspawn()
        {
        }

        protected internal abstract void Release(bool isShutdown);
    }
}
