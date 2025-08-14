//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Download
{
    public sealed class DownloadAgentHelperUpdateBytesEventArgs : GameFrameworkEventArgs
    {
        private byte[] mBytes;

        public DownloadAgentHelperUpdateBytesEventArgs()
        {
            mBytes = null;
            Offset = 0;
            Length = 0;
        }

        public int Offset
        {
            get;
            private set;
        }

        public int Length
        {
            get;
            private set;
        }

        public static DownloadAgentHelperUpdateBytesEventArgs Create(byte[] bytes, int offset, int length)
        {
            if (bytes == null)
            {
                throw new GameFrameworkException("Bytes is invalid.");
            }

            if (offset < 0 || offset >= bytes.Length)
            {
                throw new GameFrameworkException("Offset is invalid.");
            }

            if (length <= 0 || offset + length > bytes.Length)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            DownloadAgentHelperUpdateBytesEventArgs downloadAgentHelperUpdateBytesEventArgs = ReferencePool.Acquire<DownloadAgentHelperUpdateBytesEventArgs>();
            downloadAgentHelperUpdateBytesEventArgs.mBytes = bytes;
            downloadAgentHelperUpdateBytesEventArgs.Offset = offset;
            downloadAgentHelperUpdateBytesEventArgs.Length = length;
            return downloadAgentHelperUpdateBytesEventArgs;
        }

        public override void Clear()
        {
            mBytes = null;
            Offset = 0;
            Length = 0;
        }

        public byte[] GetBytes()
        {
            return mBytes;
        }
    }
}
