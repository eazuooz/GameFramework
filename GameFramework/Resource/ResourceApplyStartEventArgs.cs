//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceApplyStartEventArgs : GameFrameworkEventArgs
    {
        public ResourceApplyStartEventArgs()
        {
            ResourcePackPath = null;
            Count = 0;
            TotalLength = 0L;
        }

        public string ResourcePackPath
        {
            get;
            private set;
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

        public static ResourceApplyStartEventArgs Create(string resourcePackPath, int count, long totalLength)
        {
            ResourceApplyStartEventArgs resourceApplyStartEventArgs = ReferencePool.Acquire<ResourceApplyStartEventArgs>();
            resourceApplyStartEventArgs.ResourcePackPath = resourcePackPath;
            resourceApplyStartEventArgs.Count = count;
            resourceApplyStartEventArgs.TotalLength = totalLength;
            return resourceApplyStartEventArgs;
        }

        public override void Clear()
        {
            ResourcePackPath = null;
            Count = 0;
            TotalLength = 0L;
        }
    }
}
