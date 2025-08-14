//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameFramework.FileSystem
{
    internal sealed partial class FileSystem : IFileSystem
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct BlockData
        {
            public static readonly BlockData Empty = new BlockData(0, 0);

            private readonly int mStringIndex;
            private readonly int mClusterIndex;
            private readonly int mLength;

            public BlockData(int clusterIndex, int length)
                : this(-1, clusterIndex, length)
            {
            }

            public BlockData(int stringIndex, int clusterIndex, int length)
            {
                mStringIndex = stringIndex;
                mClusterIndex = clusterIndex;
                mLength = length;
            }

            public bool Using
            {
                get
                {
                    return mStringIndex >= 0;
                }
            }

            public int StringIndex
            {
                get
                {
                    return mStringIndex;
                }
            }

            public int ClusterIndex
            {
                get
                {
                    return mClusterIndex;
                }
            }

            public int Length
            {
                get
                {
                    return mLength;
                }
            }

            public BlockData Free()
            {
                return new BlockData(mClusterIndex, (int)GetUpBoundClusterOffset(mLength));
            }
        }
    }
}
