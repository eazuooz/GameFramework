//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceApplySuccessEventArgs : GameFrameworkEventArgs
    {
        public ResourceApplySuccessEventArgs()
        {
            Name = null;
            ApplyPath = null;
            ResourcePackPath = null;
            Length = 0;
            CompressedLength = 0;
        }

        public string Name
        {
            get;
            private set;
        }

        public string ApplyPath
        {
            get;
            private set;
        }

        public string ResourcePackPath
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

        public static ResourceApplySuccessEventArgs Create(string name, string applyPath, string resourcePackPath, int length, int compressedLength)
        {
            ResourceApplySuccessEventArgs resourceApplySuccessEventArgs = ReferencePool.Acquire<ResourceApplySuccessEventArgs>();
            resourceApplySuccessEventArgs.Name = name;
            resourceApplySuccessEventArgs.ApplyPath = applyPath;
            resourceApplySuccessEventArgs.ResourcePackPath = resourcePackPath;
            resourceApplySuccessEventArgs.Length = length;
            resourceApplySuccessEventArgs.CompressedLength = compressedLength;
            return resourceApplySuccessEventArgs;
        }

        public override void Clear()
        {
            Name = null;
            ApplyPath = null;
            ResourcePackPath = null;
            Length = 0;
            CompressedLength = 0;
        }
    }
}
