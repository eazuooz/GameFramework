//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceUpdateStartEventArgs : GameFrameworkEventArgs
    {
        public ResourceUpdateStartEventArgs()
        {
            Name = null;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLength = 0;
            CompressedLength = 0;
            RetryCount = 0;
        }

        public string Name
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

        public int CurrentLength
        {
            get;
            private set;
        }

        public int CompressedLength
        {
            get;
            private set;
        }

        public int RetryCount
        {
            get;
            private set;
        }

        public static ResourceUpdateStartEventArgs Create(string name, string downloadPath, string downloadUri, int currentLength, int compressedLength, int retryCount)
        {
            ResourceUpdateStartEventArgs resourceUpdateStartEventArgs = ReferencePool.Acquire<ResourceUpdateStartEventArgs>();
            resourceUpdateStartEventArgs.Name = name;
            resourceUpdateStartEventArgs.DownloadPath = downloadPath;
            resourceUpdateStartEventArgs.DownloadUri = downloadUri;
            resourceUpdateStartEventArgs.CurrentLength = currentLength;
            resourceUpdateStartEventArgs.CompressedLength = compressedLength;
            resourceUpdateStartEventArgs.RetryCount = retryCount;
            return resourceUpdateStartEventArgs;
        }

        public override void Clear()
        {
            Name = null;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLength = 0;
            CompressedLength = 0;
            RetryCount = 0;
        }
    }
}
