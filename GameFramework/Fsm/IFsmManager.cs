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
    public interface IFsmManager
    {
        int Count
        {
            get;
        }

        bool HasFsm<T>() where T : class;

        bool HasFsm(Type ownerType);

        bool HasFsm<T>(string name) where T : class;

        bool HasFsm(Type ownerType, string name);

        IFsm<T> GetFsm<T>() where T : class;

        FsmBase GetFsm(Type ownerType);

        IFsm<T> GetFsm<T>(string name) where T : class;

        FsmBase GetFsm(Type ownerType, string name);

        FsmBase[] GetAllFsms();

        void GetAllFsms(List<FsmBase> results);

        IFsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states) where T : class;

        IFsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class;

        IFsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states) where T : class;

        IFsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class;

        bool DestroyFsm<T>() where T : class;

        bool DestroyFsm(Type ownerType);

        bool DestroyFsm<T>(string name) where T : class;

        bool DestroyFsm(Type ownerType, string name);

        bool DestroyFsm<T>(IFsm<T> fsm) where T : class;

        bool DestroyFsm(FsmBase fsm);
    }
}
