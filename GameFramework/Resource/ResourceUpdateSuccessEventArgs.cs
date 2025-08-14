//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceUpdateSuccessEventArgs : GameFrameworkEventArgs
    {
        public ResourceUpdateSuccessEventArgs()
        {
            Name = null;
            DownloadPath = null;
            DownloadUri = null;
            Length = 0;
            CompressedLength = 0;
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

        public int Length
        {
            get;
            private set;
        }

        public int CompressedLength
        {
            get;
            private set;
        }

        public static ResourceUpdateSuccessEventArgs Create(string name, string downloadPath, string downloadUri, int length, int compressedLength)
        {
            ResourceUpdateSuccessEventArgs resourceUpdateSuccessEventArgs = ReferencePool.Acquire<ResourceUpdateSuccessEventArgs>();
            resourceUpdateSuccessEventArgs.Name = name;
            resourceUpdateSuccessEventArgs.DownloadPath = downloadPath;
            resourceUpdateSuccessEventArgs.DownloadUri = downloadUri;
            resourceUpdateSuccessEventArgs.Length = length;
            resourceUpdateSuccessEventArgs.CompressedLength = compressedLength;
            return resourceUpdateSuccessEventArgs;
        }

        public override void Clear()
        {
            Name = null;
            DownloadPath = null;
            DownloadUri = null;
            Length = 0;
            CompressedLength = 0;
        }
    }
}
