//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadResourceAgentHelperErrorEventArgs : GameFrameworkEventArgs
    {
        public LoadResourceAgentHelperErrorEventArgs()
        {
            Status = LoadResourceStatus.Success;
            ErrorMessage = null;
        }

        public LoadResourceStatus Status
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public static LoadResourceAgentHelperErrorEventArgs Create(LoadResourceStatus status, string errorMessage)
        {
            LoadResourceAgentHelperErrorEventArgs loadResourceAgentHelperErrorEventArgs = ReferencePool.Acquire<LoadResourceAgentHelperErrorEventArgs>();
            loadResourceAgentHelperErrorEventArgs.Status = status;
            loadResourceAgentHelperErrorEventArgs.ErrorMessage = errorMessage;
            return loadResourceAgentHelperErrorEventArgs;
        }

        public override void Clear()
        {
            Status = LoadResourceStatus.Success;
            ErrorMessage = null;
        }
    }
}
