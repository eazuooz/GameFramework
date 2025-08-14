//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameFramework.Resource
{
    public partial struct PackageVersionList
    {
        [StructLayout(LayoutKind.Auto)]
        public struct Asset
        {
            private static readonly int[] EmptyIntArray = new int[] { };

            private readonly string mName;
            private readonly int[] mDependencyAssetIndexes;

            public Asset(string name, int[] dependencyAssetIndexes)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                mName = name;
                mDependencyAssetIndexes = dependencyAssetIndexes ?? EmptyIntArray;
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public int[] GetDependencyAssetIndexes()
            {
                return mDependencyAssetIndexes;
            }
        }
    }
}
