//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Download
{
    public sealed class DownloadAgentHelperUpdateLengthEventArgs : GameFrameworkEventArgs
    {
        public DownloadAgentHelperUpdateLengthEventArgs()
        {
            DeltaLength = 0;
        }

        public int DeltaLength
        {
            get;
            private set;
        }

        public static DownloadAgentHelperUpdateLengthEventArgs Create(int deltaLength)
        {
            if (deltaLength <= 0)
            {
                throw new GameFrameworkException("Delta length is invalid.");
            }

            DownloadAgentHelperUpdateLengthEventArgs downloadAgentHelperUpdateLengthEventArgs = ReferencePool.Acquire<DownloadAgentHelperUpdateLengthEventArgs>();
            downloadAgentHelperUpdateLengthEventArgs.DeltaLength = deltaLength;
            return downloadAgentHelperUpdateLengthEventArgs;
        }

        public override void Clear()
        {
            DeltaLength = 0;
        }
    }
}
