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
    [StructLayout(LayoutKind.Auto)]
    public partial struct LocalVersionList
    {
        private static readonly Resource[] EmptyResourceArray = new Resource[] { };
        private static readonly FileSystem[] EmptyFileSystemArray = new FileSystem[] { };

        private readonly bool mIsValid;
        private readonly Resource[] mResources;
        private readonly FileSystem[] mFileSystems;

        public LocalVersionList(Resource[] resources, FileSystem[] fileSystems)
        {
            mIsValid = true;
            mResources = resources ?? EmptyResourceArray;
            mFileSystems = fileSystems ?? EmptyFileSystemArray;
        }

        public bool IsValid
        {
            get
            {
                return mIsValid;
            }
        }

        public Resource[] GetResources()
        {
            if (!mIsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return mResources;
        }

        public FileSystem[] GetFileSystems()
        {
            if (!mIsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return mFileSystems;
        }
    }
}
