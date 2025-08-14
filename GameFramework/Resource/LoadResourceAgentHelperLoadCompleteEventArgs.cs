//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadResourceAgentHelperLoadCompleteEventArgs : GameFrameworkEventArgs
    {
        public LoadResourceAgentHelperLoadCompleteEventArgs()
        {
            Asset = null;
        }

        public object Asset
        {
            get;
            private set;
        }

        public static LoadResourceAgentHelperLoadCompleteEventArgs Create(object asset)
        {
            LoadResourceAgentHelperLoadCompleteEventArgs loadResourceAgentHelperLoadCompleteEventArgs = ReferencePool.Acquire<LoadResourceAgentHelperLoadCompleteEventArgs>();
            loadResourceAgentHelperLoadCompleteEventArgs.Asset = asset;
            return loadResourceAgentHelperLoadCompleteEventArgs;
        }

        public override void Clear()
        {
            Asset = null;
        }
    }
}
