//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceUpdateFailureEventArgs : GameFrameworkEventArgs
    {
        public ResourceUpdateFailureEventArgs()
        {
            Name = null;
            DownloadUri = null;
            RetryCount = 0;
            TotalRetryCount = 0;
            ErrorMessage = null;
        }

        public string Name
        {
            get;
            private set;
        }

        public string DownloadUri
        {
            get;
            private set;
        }

        public int RetryCount
        {
            get;
            private set;
        }

        public int TotalRetryCount
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public static ResourceUpdateFailureEventArgs Create(string name, string downloadUri, int retryCount, int totalRetryCount, string errorMessage)
        {
            ResourceUpdateFailureEventArgs resourceUpdateFailureEventArgs = ReferencePool.Acquire<ResourceUpdateFailureEventArgs>();
            resourceUpdateFailureEventArgs.Name = name;
            resourceUpdateFailureEventArgs.DownloadUri = downloadUri;
            resourceUpdateFailureEventArgs.RetryCount = retryCount;
            resourceUpdateFailureEventArgs.TotalRetryCount = totalRetryCount;
            resourceUpdateFailureEventArgs.ErrorMessage = errorMessage;
            return resourceUpdateFailureEventArgs;
        }

        public override void Clear()
        {
            Name = null;
            DownloadUri = null;
            RetryCount = 0;
            TotalRetryCount = 0;
            ErrorMessage = null;
        }
    }
}
