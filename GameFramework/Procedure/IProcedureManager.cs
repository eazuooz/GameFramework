//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Fsm;
using System;

namespace GameFramework.Procedure
{
    public interface IProcedureManager
    {
        ProcedureBase CurrentProcedure
        {
            get;
        }

        float CurrentProcedureTime
        {
            get;
        }

        void Initialize(IFsmManager fsmManager, params ProcedureBase[] procedures);

        void StartProcedure<T>() where T : ProcedureBase;

        void StartProcedure(Type procedureType);

        bool HasProcedure<T>() where T : ProcedureBase;

        bool HasProcedure(Type procedureType);

        ProcedureBase GetProcedure<T>() where T : ProcedureBase;

        ProcedureBase GetProcedure(Type procedureType);
    }
}
