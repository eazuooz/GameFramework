//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public sealed class ResourceApplyFailureEventArgs : GameFrameworkEventArgs
    {
        public ResourceApplyFailureEventArgs()
        {
            Name = null;
            ResourcePackPath = null;
            ErrorMessage = null;
        }

        public string Name
        {
            get;
            private set;
        }

        public string ResourcePackPath
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public static ResourceApplyFailureEventArgs Create(string name, string resourcePackPath, string errorMessage)
        {
            ResourceApplyFailureEventArgs resourceApplyFailureEventArgs = ReferencePool.Acquire<ResourceApplyFailureEventArgs>();
            resourceApplyFailureEventArgs.Name = name;
            resourceApplyFailureEventArgs.ResourcePackPath = resourcePackPath;
            resourceApplyFailureEventArgs.ErrorMessage = errorMessage;
            return resourceApplyFailureEventArgs;
        }

        public override void Clear()
        {
            Name = null;
            ResourcePackPath = null;
            ErrorMessage = null;
        }
    }
}
