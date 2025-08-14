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
        public struct ResourceGroup
        {
            private static readonly int[] EmptyIntArray = new int[] { };

            private readonly string mName;
            private readonly int[] mResourceIndexes;

            public ResourceGroup(string name, int[] resourceIndexes)
            {
                if (name == null)
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                mName = name;
                mResourceIndexes = resourceIndexes ?? EmptyIntArray;
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public int[] GetResourceIndexes()
            {
                return mResourceIndexes;
            }
        }
    }
}
