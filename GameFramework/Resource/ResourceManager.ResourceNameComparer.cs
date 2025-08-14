//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed class ResourceNameComparer : IComparer<ResourceName>, IEqualityComparer<ResourceName>
        {
            public int Compare(ResourceName x, ResourceName y)
            {
                return x.CompareTo(y);
            }

            public bool Equals(ResourceName x, ResourceName y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(ResourceName obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
