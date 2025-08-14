//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed class AssetInfo
        {
            private readonly string mAssetName;
            private readonly ResourceName mResourceName;
            private readonly string[] mDependencyAssetNames;

            public AssetInfo(string assetName, ResourceName resourceName, string[] dependencyAssetNames)
            {
                mAssetName = assetName;
                mResourceName = resourceName;
                mDependencyAssetNames = dependencyAssetNames;
            }

            public string AssetName
            {
                get
                {
                    return mAssetName;
                }
            }

            public ResourceName ResourceName
            {
                get
                {
                    return mResourceName;
                }
            }

            public string[] GetDependencyAssetNames()
            {
                return mDependencyAssetNames;
            }
        }
    }
}
