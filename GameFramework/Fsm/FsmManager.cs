//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework.Fsm
{
    internal sealed class FsmManager : GameFrameworkModule, IFsmManager
    {
        private readonly Dictionary<TypeNamePair, FsmBase> mFsms;
        private readonly List<FsmBase> mTempFsms;

        public FsmManager()
        {
            mFsms = new Dictionary<TypeNamePair, FsmBase>();
            mTempFsms = new List<FsmBase>();
        }

        internal override int Priority
        {
            get
            {
                return 1;
            }
        }

        public int Count
        {
            get
            {
                return mFsms.Count;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            mTempFsms.Clear();
            if (mFsms.Count <= 0)
            {
                return;
            }

            foreach (KeyValuePair<TypeNamePair, FsmBase> fsm in mFsms)
            {
                mTempFsms.Add(fsm.Value);
            }

            foreach (FsmBase fsm in mTempFsms)
            {
                if (fsm.IsDestroyed)
                {
                    continue;
                }

                fsm.Update(elapseSeconds, realElapseSeconds);
            }
        }

        internal override void Shutdown()
        {
            foreach (KeyValuePair<TypeNamePair, FsmBase> fsm in mFsms)
            {
                fsm.Value.Shutdown();
            }

            mFsms.Clear();
            mTempFsms.Clear();
        }

        public bool HasFsm<T>() where T : class
        {
            return InternalHasFsm(new TypeNamePair(typeof(T)));
        }

        public bool HasFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new GameFrameworkException("Owner type is invalid.");
            }

            return InternalHasFsm(new TypeNamePair(ownerType));
        }

        public bool HasFsm<T>(string name) where T : class
        {
            return InternalHasFsm(new TypeNamePair(typeof(T), name));
        }

        public bool HasFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                throw new GameFrameworkException("Owner type is invalid.");
            }

            return InternalHasFsm(new TypeNamePair(ownerType, name));
        }

        public IFsm<T> GetFsm<T>() where T : class
        {
            return (IFsm<T>)InternalGetFsm(new TypeNamePair(typeof(T)));
        }

        public FsmBase GetFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new GameFrameworkException("Owner type is invalid.");
            }

            return InternalGetFsm(new TypeNamePair(ownerType));
        }

        public IFsm<T> GetFsm<T>(string name) where T : class
        {
            return (IFsm<T>)InternalGetFsm(new TypeNamePair(typeof(T), name));
        }

        public FsmBase GetFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                throw new GameFrameworkException("Owner type is invalid.");
            }

            return InternalGetFsm(new TypeNamePair(ownerType, name));
        }

        public FsmBase[] GetAllFsms()
        {
            int index = 0;
            FsmBase[] results = new FsmBase[mFsms.Count];
            foreach (KeyValuePair<TypeNamePair, FsmBase> fsm in mFsms)
            {
                results[index++] = fsm.Value;
            }

            return results;
        }

        public void GetAllFsms(List<FsmBase> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<TypeNamePair, FsmBase> fsm in mFsms)
            {
                results.Add(fsm.Value);
            }
        }

        public IFsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }

        public IFsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class
        {
            TypeNamePair typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasFsm<T>(name))
            {
                throw new GameFrameworkException(Utility.Text.Format("Already exist FSM '{0}'.", typeNamePair));
            }

            Fsm<T> fsm = Fsm<T>.Create(name, owner, states);
            mFsms.Add(typeNamePair, fsm);
            return fsm;
        }

        public IFsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }

        public IFsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class
        {
            TypeNamePair typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasFsm<T>(name))
            {
                throw new GameFrameworkException(Utility.Text.Format("Already exist FSM '{0}'.", typeNamePair));
            }

            Fsm<T> fsm = Fsm<T>.Create(name, owner, states);
            mFsms.Add(typeNamePair, fsm);
            return fsm;
        }

        public bool DestroyFsm<T>() where T : class
        {
            return InternalDestroyFsm(new TypeNamePair(typeof(T)));
        }

        public bool DestroyFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new GameFrameworkException("Owner type is invalid.");
            }

            return InternalDestroyFsm(new TypeNamePair(ownerType));
        }

        public bool DestroyFsm<T>(string name) where T : class
        {
            return InternalDestroyFsm(new TypeNamePair(typeof(T), name));
        }

        public bool DestroyFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                throw new GameFrameworkException("Owner type is invalid.");
            }

            return InternalDestroyFsm(new TypeNamePair(ownerType, name));
        }

        public bool DestroyFsm<T>(IFsm<T> fsm) where T : class
        {
            if (fsm == null)
            {
                throw new GameFrameworkException("FSM is invalid.");
            }

            return InternalDestroyFsm(new TypeNamePair(typeof(T), fsm.Name));
        }

        public bool DestroyFsm(FsmBase fsm)
        {
            if (fsm == null)
            {
                throw new GameFrameworkException("FSM is invalid.");
            }

            return InternalDestroyFsm(new TypeNamePair(fsm.OwnerType, fsm.Name));
        }

        private bool InternalHasFsm(TypeNamePair typeNamePair)
        {
            return mFsms.ContainsKey(typeNamePair);
        }

        private FsmBase InternalGetFsm(TypeNamePair typeNamePair)
        {
            FsmBase fsm = null;
            if (mFsms.TryGetValue(typeNamePair, out fsm))
            {
                return fsm;
            }

            return null;
        }

        private bool InternalDestroyFsm(TypeNamePair typeNamePair)
        {
            FsmBase fsm = null;
            if (mFsms.TryGetValue(typeNamePair, out fsm))
            {
                fsm.Shutdown();
                return mFsms.Remove(typeNamePair);
            }

            return false;
        }
    }
}
