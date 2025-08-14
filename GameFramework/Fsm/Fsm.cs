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
    internal sealed class Fsm<T> : FsmBase, IReference, IFsm<T> where T : class
    {
        private T mOwner;
        private readonly Dictionary<Type, FsmState<T>> mStates;
        private Dictionary<string, Variable> mDatas;
        private FsmState<T> mCurrentState;
        private float mCurrentStateTime;
        private bool mIsDestroyed;

        public Fsm()
        {
            mOwner = null;
            mStates = new Dictionary<Type, FsmState<T>>();
            mDatas = null;
            mCurrentState = null;
            mCurrentStateTime = 0f;
            mIsDestroyed = true;
        }

        public T Owner
        {
            get
            {
                return mOwner;
            }
        }

        public override Type OwnerType
        {
            get
            {
                return typeof(T);
            }
        }

        public override int FsmStateCount
        {
            get
            {
                return mStates.Count;
            }
        }

        public override bool IsRunning
        {
            get
            {
                return mCurrentState != null;
            }
        }

        public override bool IsDestroyed
        {
            get
            {
                return mIsDestroyed;
            }
        }

        public FsmState<T> CurrentState
        {
            get
            {
                return mCurrentState;
            }
        }

        public override string CurrentStateName
        {
            get
            {
                return mCurrentState != null ? mCurrentState.GetType().FullName : null;
            }
        }

        public override float CurrentStateTime
        {
            get
            {
                return mCurrentStateTime;
            }
        }

        public static Fsm<T> Create(string name, T owner, params FsmState<T>[] states)
        {
            if (owner == null)
            {
                throw new GameFrameworkException("FSM owner is invalid.");
            }

            if (states == null || states.Length < 1)
            {
                throw new GameFrameworkException("FSM states is invalid.");
            }

            Fsm<T> fsm = ReferencePool.Acquire<Fsm<T>>();
            fsm.Name = name;
            fsm.mOwner = owner;
            fsm.mIsDestroyed = false;
            foreach (FsmState<T> state in states)
            {
                if (state == null)
                {
                    throw new GameFrameworkException("FSM states is invalid.");
                }

                Type stateType = state.GetType();
                if (fsm.mStates.ContainsKey(stateType))
                {
                    throw new GameFrameworkException(Utility.Text.Format("FSM '{0}' state '{1}' is already exist.", new TypeNamePair(typeof(T), name), stateType.FullName));
                }

                fsm.mStates.Add(stateType, state);
                state.OnInit(fsm);
            }

            return fsm;
        }

        public static Fsm<T> Create(string name, T owner, List<FsmState<T>> states)
        {
            if (owner == null)
            {
                throw new GameFrameworkException("FSM owner is invalid.");
            }

            if (states == null || states.Count < 1)
            {
                throw new GameFrameworkException("FSM states is invalid.");
            }

            Fsm<T> fsm = ReferencePool.Acquire<Fsm<T>>();
            fsm.Name = name;
            fsm.mOwner = owner;
            fsm.mIsDestroyed = false;
            foreach (FsmState<T> state in states)
            {
                if (state == null)
                {
                    throw new GameFrameworkException("FSM states is invalid.");
                }

                Type stateType = state.GetType();
                if (fsm.mStates.ContainsKey(stateType))
                {
                    throw new GameFrameworkException(Utility.Text.Format("FSM '{0}' state '{1}' is already exist.", new TypeNamePair(typeof(T), name), stateType.FullName));
                }

                fsm.mStates.Add(stateType, state);
                state.OnInit(fsm);
            }

            return fsm;
        }

        public void Clear()
        {
            if (mCurrentState != null)
            {
                mCurrentState.OnLeave(this, true);
            }

            foreach (KeyValuePair<Type, FsmState<T>> state in mStates)
            {
                state.Value.OnDestroy(this);
            }

            Name = null;
            mOwner = null;
            mStates.Clear();

            if (mDatas != null)
            {
                foreach (KeyValuePair<string, Variable> data in mDatas)
                {
                    if (data.Value == null)
                    {
                        continue;
                    }

                    ReferencePool.Release(data.Value);
                }

                mDatas.Clear();
            }

            mCurrentState = null;
            mCurrentStateTime = 0f;
            mIsDestroyed = true;
        }

        public void Start<TState>() where TState : FsmState<T>
        {
            if (IsRunning)
            {
                throw new GameFrameworkException("FSM is running, can not start again.");
            }

            FsmState<T> state = GetState<TState>();
            if (state == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("FSM '{0}' can not start state '{1}' which is not exist.", new TypeNamePair(typeof(T), Name), typeof(TState).FullName));
            }

            mCurrentStateTime = 0f;
            mCurrentState = state;
            mCurrentState.OnEnter(this);
        }

        public void Start(Type stateType)
        {
            if (IsRunning)
            {
                throw new GameFrameworkException("FSM is running, can not start again.");
            }

            if (stateType == null)
            {
                throw new GameFrameworkException("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new GameFrameworkException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName));
            }

            FsmState<T> state = GetState(stateType);
            if (state == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("FSM '{0}' can not start state '{1}' which is not exist.", new TypeNamePair(typeof(T), Name), stateType.FullName));
            }

            mCurrentStateTime = 0f;
            mCurrentState = state;
            mCurrentState.OnEnter(this);
        }

        public bool HasState<TState>() where TState : FsmState<T>
        {
            return mStates.ContainsKey(typeof(TState));
        }

        public bool HasState(Type stateType)
        {
            if (stateType == null)
            {
                throw new GameFrameworkException("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new GameFrameworkException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName));
            }

            return mStates.ContainsKey(stateType);
        }

        public TState GetState<TState>() where TState : FsmState<T>
        {
            FsmState<T> state = null;
            if (mStates.TryGetValue(typeof(TState), out state))
            {
                return (TState)state;
            }

            return null;
        }

        public FsmState<T> GetState(Type stateType)
        {
            if (stateType == null)
            {
                throw new GameFrameworkException("State type is invalid.");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new GameFrameworkException(Utility.Text.Format("State type '{0}' is invalid.", stateType.FullName));
            }

            FsmState<T> state = null;
            if (mStates.TryGetValue(stateType, out state))
            {
                return state;
            }

            return null;
        }

        public FsmState<T>[] GetAllStates()
        {
            int index = 0;
            FsmState<T>[] results = new FsmState<T>[mStates.Count];
            foreach (KeyValuePair<Type, FsmState<T>> state in mStates)
            {
                results[index++] = state.Value;
            }

            return results;
        }

        public void GetAllStates(List<FsmState<T>> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<Type, FsmState<T>> state in mStates)
            {
                results.Add(state.Value);
            }
        }

        public bool HasData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Data name is invalid.");
            }

            if (mDatas == null)
            {
                return false;
            }

            return mDatas.ContainsKey(name);
        }

        public TData GetData<TData>(string name) where TData : Variable
        {
            return (TData)GetData(name);
        }

        public Variable GetData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Data name is invalid.");
            }

            if (mDatas == null)
            {
                return null;
            }

            Variable data = null;
            if (mDatas.TryGetValue(name, out data))
            {
                return data;
            }

            return null;
        }

        public void SetData<TData>(string name, TData data) where TData : Variable
        {
            SetData(name, (Variable)data);
        }

        public void SetData(string name, Variable data)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Data name is invalid.");
            }

            if (mDatas == null)
            {
                mDatas = new Dictionary<string, Variable>(StringComparer.Ordinal);
            }

            Variable oldData = GetData(name);
            if (oldData != null)
            {
                ReferencePool.Release(oldData);
            }

            mDatas[name] = data;
        }

        public bool RemoveData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Data name is invalid.");
            }

            if (mDatas == null)
            {
                return false;
            }

            Variable oldData = GetData(name);
            if (oldData != null)
            {
                ReferencePool.Release(oldData);
            }

            return mDatas.Remove(name);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (mCurrentState == null)
            {
                return;
            }

            mCurrentStateTime += elapseSeconds;
            mCurrentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
        }

        internal override void Shutdown()
        {
            ReferencePool.Release(this);
        }

        internal void ChangeState<TState>() where TState : FsmState<T>
        {
            ChangeState(typeof(TState));
        }

        internal void ChangeState(Type stateType)
        {
            if (mCurrentState == null)
            {
                throw new GameFrameworkException("Current state is invalid.");
            }

            FsmState<T> state = GetState(stateType);
            if (state == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("FSM '{0}' can not change state to '{1}' which is not exist.", new TypeNamePair(typeof(T), Name), stateType.FullName));
            }

            mCurrentState.OnLeave(this, false);
            mCurrentStateTime = 0f;
            mCurrentState = state;
            mCurrentState.OnEnter(this);
        }
    }
}
