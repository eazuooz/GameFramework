//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Download
{
    public sealed class DownloadAgentHelperErrorEventArgs : GameFrameworkEventArgs
    {
        public DownloadAgentHelperErrorEventArgs()
        {
            DeleteDownloading = false;
            ErrorMessage = null;
        }

        public bool DeleteDownloading
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public static DownloadAgentHelperErrorEventArgs Create(bool deleteDownloading, string errorMessage)
        {
            DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = ReferencePool.Acquire<DownloadAgentHelperErrorEventArgs>();
            downloadAgentHelperErrorEventArgs.DeleteDownloading = deleteDownloading;
            downloadAgentHelperErrorEventArgs.ErrorMessage = errorMessage;
            return downloadAgentHelperErrorEventArgs;
        }

        public override void Clear()
        {
            DeleteDownloading = false;
            ErrorMessage = null;
        }
    }
}
