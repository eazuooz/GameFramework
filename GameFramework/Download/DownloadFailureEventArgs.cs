//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Download
{
    public sealed class DownloadFailureEventArgs : GameFrameworkEventArgs
    {
        public DownloadFailureEventArgs()
        {
            SerialId = 0;
            DownloadPath = null;
            DownloadUri = null;
            ErrorMessage = null;
            UserData = null;
        }

        public int SerialId
        {
            get;
            private set;
        }

        public string DownloadPath
        {
            get;
            private set;
        }

        public string DownloadUri
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static DownloadFailureEventArgs Create(int serialId, string downloadPath, string downloadUri, string errorMessage, object userData)
        {
            DownloadFailureEventArgs downloadFailureEventArgs = ReferencePool.Acquire<DownloadFailureEventArgs>();
            downloadFailureEventArgs.SerialId = serialId;
            downloadFailureEventArgs.DownloadPath = downloadPath;
            downloadFailureEventArgs.DownloadUri = downloadUri;
            downloadFailureEventArgs.ErrorMessage = errorMessage;
            downloadFailureEventArgs.UserData = userData;
            return downloadFailureEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            DownloadPath = null;
            DownloadUri = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
