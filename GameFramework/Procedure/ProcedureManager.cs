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
    internal sealed class ProcedureManager : GameFrameworkModule, IProcedureManager
    {
        private IFsmManager mFsmManager;
        private IFsm<IProcedureManager> mProcedureFsm;

        public ProcedureManager()
        {
            mFsmManager = null;
            mProcedureFsm = null;
        }

        internal override int Priority
        {
            get
            {
                return -2;
            }
        }

        public ProcedureBase CurrentProcedure
        {
            get
            {
                if (mProcedureFsm == null)
                {
                    throw new GameFrameworkException("You must initialize procedure first.");
                }

                return (ProcedureBase)mProcedureFsm.CurrentState;
            }
        }

        public float CurrentProcedureTime
        {
            get
            {
                if (mProcedureFsm == null)
                {
                    throw new GameFrameworkException("You must initialize procedure first.");
                }

                return mProcedureFsm.CurrentStateTime;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            if (mFsmManager != null)
            {
                if (mProcedureFsm != null)
                {
                    mFsmManager.DestroyFsm(mProcedureFsm);
                    mProcedureFsm = null;
                }

                mFsmManager = null;
            }
        }

        public void Initialize(IFsmManager fsmManager, params ProcedureBase[] procedures)
        {
            if (fsmManager == null)
            {
                throw new GameFrameworkException("FSM manager is invalid.");
            }

            mFsmManager = fsmManager;
            mProcedureFsm = mFsmManager.CreateFsm(this, procedures);
        }

        public void StartProcedure<T>() where T : ProcedureBase
        {
            if (mProcedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            mProcedureFsm.Start<T>();
        }

        public void StartProcedure(Type procedureType)
        {
            if (mProcedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            mProcedureFsm.Start(procedureType);
        }

        public bool HasProcedure<T>() where T : ProcedureBase
        {
            if (mProcedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            return mProcedureFsm.HasState<T>();
        }

        public bool HasProcedure(Type procedureType)
        {
            if (mProcedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            return mProcedureFsm.HasState(procedureType);
        }

        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            if (mProcedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            return mProcedureFsm.GetState<T>();
        }

        public ProcedureBase GetProcedure(Type procedureType)
        {
            if (mProcedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            return (ProcedureBase)mProcedureFsm.GetState(procedureType);
        }
    }
}
