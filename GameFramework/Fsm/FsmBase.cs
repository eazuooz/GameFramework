//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.Fsm
{
    public abstract class FsmBase
    {
        private string mName;

        public FsmBase()
        {
            mName = string.Empty;
        }

        public string Name
        {
            get
            {
                return mName;
            }
            protected set
            {
                mName = value ?? string.Empty;
            }
        }

        public string FullName
        {
            get
            {
                return new TypeNamePair(OwnerType, mName).ToString();
            }
        }

        public abstract Type OwnerType
        {
            get;
        }

        public abstract int FsmStateCount
        {
            get;
        }

        public abstract bool IsRunning
        {
            get;
        }

        public abstract bool IsDestroyed
        {
            get;
        }

        public abstract string CurrentStateName
        {
            get;
        }

        public abstract float CurrentStateTime
        {
            get;
        }

        internal abstract void Update(float elapseSeconds, float realElapseSeconds);

        internal abstract void Shutdown();
    }
}
