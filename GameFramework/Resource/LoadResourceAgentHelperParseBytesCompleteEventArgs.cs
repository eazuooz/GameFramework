//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadResourceAgentHelperParseBytesCompleteEventArgs : GameFrameworkEventArgs
    {
        public LoadResourceAgentHelperParseBytesCompleteEventArgs()
        {
            Resource = null;
        }

        public object Resource
        {
            get;
            private set;
        }

        public static LoadResourceAgentHelperParseBytesCompleteEventArgs Create(object resource)
        {
            LoadResourceAgentHelperParseBytesCompleteEventArgs loadResourceAgentHelperParseBytesCompleteEventArgs = ReferencePool.Acquire<LoadResourceAgentHelperParseBytesCompleteEventArgs>();
            loadResourceAgentHelperParseBytesCompleteEventArgs.Resource = resource;
            return loadResourceAgentHelperParseBytesCompleteEventArgs;
        }

        public override void Clear()
        {
            Resource = null;
        }
    }
}
