//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceVerifyFailureEventArgs : GameFrameworkEventArgs
    {
        public ResourceVerifyFailureEventArgs()
        {
            Name = null;
        }

        public string Name
        {
            get;
            private set;
        }

        public static ResourceVerifyFailureEventArgs Create(string name)
        {
            ResourceVerifyFailureEventArgs resourceVerifyFailureEventArgs = ReferencePool.Acquire<ResourceVerifyFailureEventArgs>();
            resourceVerifyFailureEventArgs.Name = name;
            return resourceVerifyFailureEventArgs;
        }

        public override void Clear()
        {
            Name = null;
        }
    }
}
