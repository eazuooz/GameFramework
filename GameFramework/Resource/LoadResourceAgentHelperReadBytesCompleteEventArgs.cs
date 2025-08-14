//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class LoadResourceAgentHelperReadBytesCompleteEventArgs : GameFrameworkEventArgs
    {
        private byte[] mBytes;

        public LoadResourceAgentHelperReadBytesCompleteEventArgs()
        {
            mBytes = null;
        }

        public static LoadResourceAgentHelperReadBytesCompleteEventArgs Create(byte[] bytes)
        {
            LoadResourceAgentHelperReadBytesCompleteEventArgs loadResourceAgentHelperReadBytesCompleteEventArgs = ReferencePool.Acquire<LoadResourceAgentHelperReadBytesCompleteEventArgs>();
            loadResourceAgentHelperReadBytesCompleteEventArgs.mBytes = bytes;
            return loadResourceAgentHelperReadBytesCompleteEventArgs;
        }

        public override void Clear()
        {
            mBytes = null;
        }

        public byte[] GetBytes()
        {
            return mBytes;
        }
    }
}
