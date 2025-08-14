//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadResourceAgentHelperUpdateEventArgs : GameFrameworkEventArgs
    {
        public LoadResourceAgentHelperUpdateEventArgs()
        {
            Type = LoadResourceProgress.Unknown;
            Progress = 0f;
        }

        public LoadResourceProgress Type
        {
            get;
            private set;
        }

        public float Progress
        {
            get;
            private set;
        }

        public static LoadResourceAgentHelperUpdateEventArgs Create(LoadResourceProgress type, float progress)
        {
            LoadResourceAgentHelperUpdateEventArgs loadResourceAgentHelperUpdateEventArgs = ReferencePool.Acquire<LoadResourceAgentHelperUpdateEventArgs>();
            loadResourceAgentHelperUpdateEventArgs.Type = type;
            loadResourceAgentHelperUpdateEventArgs.Progress = progress;
            return loadResourceAgentHelperUpdateEventArgs;
        }

        public override void Clear()
        {
            Type = LoadResourceProgress.Unknown;
            Progress = 0f;
        }
    }
}
