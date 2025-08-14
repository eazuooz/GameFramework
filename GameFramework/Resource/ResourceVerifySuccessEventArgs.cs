//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceVerifySuccessEventArgs : GameFrameworkEventArgs
    {
        public ResourceVerifySuccessEventArgs()
        {
            Name = null;
            Length = 0;
        }

        public string Name
        {
            get;
            private set;
        }

        public int Length
        {
            get;
            private set;
        }

        public static ResourceVerifySuccessEventArgs Create(string name, int length)
        {
            ResourceVerifySuccessEventArgs resourceVerifySuccessEventArgs = ReferencePool.Acquire<ResourceVerifySuccessEventArgs>();
            resourceVerifySuccessEventArgs.Name = name;
            resourceVerifySuccessEventArgs.Length = length;
            return resourceVerifySuccessEventArgs;
        }

        public override void Clear()
        {
            Name = null;
            Length = 0;
        }
    }
}
