//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadResourceAgentHelperReadFileCompleteEventArgs : GameFrameworkEventArgs
    {
        public LoadResourceAgentHelperReadFileCompleteEventArgs()
        {
            Resource = null;
        }

        public object Resource
        {
            get;
            private set;
        }

        public static LoadResourceAgentHelperReadFileCompleteEventArgs Create(object resource)
        {
            LoadResourceAgentHelperReadFileCompleteEventArgs loadResourceAgentHelperReadFileCompleteEventArgs = ReferencePool.Acquire<LoadResourceAgentHelperReadFileCompleteEventArgs>();
            loadResourceAgentHelperReadFileCompleteEventArgs.Resource = resource;
            return loadResourceAgentHelperReadFileCompleteEventArgs;
        }

        public override void Clear()
        {
            Resource = null;
        }
    }
}
