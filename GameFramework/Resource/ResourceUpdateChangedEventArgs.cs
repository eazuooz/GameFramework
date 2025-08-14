//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceUpdateChangedEventArgs : GameFrameworkEventArgs
    {
        public ResourceUpdateChangedEventArgs()
        {
            Name = null;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLength = 0;
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

        public static ResourceUpdateChangedEventArgs Create(string name, string downloadPath, string downloadUri, int currentLength, int compressedLength)
        {
            ResourceUpdateChangedEventArgs resourceUpdateChangedEventArgs = ReferencePool.Acquire<ResourceUpdateChangedEventArgs>();
            resourceUpdateChangedEventArgs.Name = name;
            resourceUpdateChangedEventArgs.DownloadPath = downloadPath;
            resourceUpdateChangedEventArgs.DownloadUri = downloadUri;
            resourceUpdateChangedEventArgs.CurrentLength = currentLength;
            resourceUpdateChangedEventArgs.CompressedLength = compressedLength;
            return resourceUpdateChangedEventArgs;
        }

        public override void Clear()
        {
            Name = null;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLength = 0;
            CompressedLength = 0;
        }
    }
}
