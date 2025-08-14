//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceVerifyStartEventArgs : GameFrameworkEventArgs
    {
        public ResourceVerifyStartEventArgs()
        {
            Count = 0;
            TotalLength = 0L;
        }

        public int Count
        {
            get;
            private set;
        }

        public long TotalLength
        {
            get;
            private set;
        }

        public static ResourceVerifyStartEventArgs Create(int count, long totalLength)
        {
            ResourceVerifyStartEventArgs resourceVerifyStartEventArgs = ReferencePool.Acquire<ResourceVerifyStartEventArgs>();
            resourceVerifyStartEventArgs.Count = count;
            resourceVerifyStartEventArgs.TotalLength = totalLength;
            return resourceVerifyStartEventArgs;
        }

        public override void Clear()
        {
            Count = 0;
            TotalLength = 0L;
        }
    }
}
