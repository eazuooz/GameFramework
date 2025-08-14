//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Download
{
    public sealed class DownloadAgentHelperCompleteEventArgs : GameFrameworkEventArgs
    {
        public DownloadAgentHelperCompleteEventArgs()
        {
            Length = 0L;
        }

        public long Length
        {
            get;
            private set;
        }

        public static DownloadAgentHelperCompleteEventArgs Create(long length)
        {
            if (length < 0L)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            DownloadAgentHelperCompleteEventArgs downloadAgentHelperCompleteEventArgs = ReferencePool.Acquire<DownloadAgentHelperCompleteEventArgs>();
            downloadAgentHelperCompleteEventArgs.Length = length;
            return downloadAgentHelperCompleteEventArgs;
        }

        public override void Clear()
        {
            Length = 0L;
        }
    }
}
