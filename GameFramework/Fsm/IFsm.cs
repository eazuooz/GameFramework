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
    public interface IFsm<T> where T : class
    {
        string Name
        {
            get;
        }

        string FullName
        {
            get;
        }

        T Owner
        {
            get;
        }

        int FsmStateCount
        {
            get;
        }

        bool IsRunning
        {
            get;
        }

        bool IsDestroyed
        {
            get;
        }

        FsmState<T> CurrentState
        {
            get;
        }

        float CurrentStateTime
        {
            get;
        }

        void Start<TState>() where TState : FsmState<T>;

        void Start(Type stateType);

        bool HasState<TState>() where TState : FsmState<T>;

        bool HasState(Type stateType);

        TState GetState<TState>() where TState : FsmState<T>;

        FsmState<T> GetState(Type stateType);

        FsmState<T>[] GetAllStates();

        void GetAllStates(List<FsmState<T>> results);

        bool HasData(string name);

        TData GetData<TData>(string name) where TData : Variable;

        Variable GetData(string name);

        void SetData<TData>(string name, TData data) where TData : Variable;

        void SetData(string name, Variable data);

        bool RemoveData(string name);
    }
}
