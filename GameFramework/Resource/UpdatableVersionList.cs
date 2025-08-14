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
    public partial struct UpdatableVersionList
    {
        private static readonly Asset[] EmptyAssetArray = new Asset[] { };
        private static readonly Resource[] EmptyResourceArray = new Resource[] { };
        private static readonly FileSystem[] EmptyFileSystemArray = new FileSystem[] { };
        private static readonly ResourceGroup[] EmptyResourceGroupArray = new ResourceGroup[] { };

        private readonly bool mIsValid;
        private readonly string mApplicableGameVersion;
        private readonly int mInternalResourceVersion;
        private readonly Asset[] mAssets;
        private readonly Resource[] mResources;
        private readonly FileSystem[] mFileSystems;
        private readonly ResourceGroup[] mResourceGroups;

        public UpdatableVersionList(string applicableGameVersion, int internalResourceVersion, Asset[] assets, Resource[] resources, FileSystem[] fileSystems, ResourceGroup[] resourceGroups)
        {
            mIsValid = true;
            mApplicableGameVersion = applicableGameVersion;
            mInternalResourceVersion = internalResourceVersion;
            mAssets = assets ?? EmptyAssetArray;
            mResources = resources ?? EmptyResourceArray;
            mFileSystems = fileSystems ?? EmptyFileSystemArray;
            mResourceGroups = resourceGroups ?? EmptyResourceGroupArray;
        }

        public bool IsValid
        {
            get
            {
                return mIsValid;
            }
        }

        public string ApplicableGameVersion
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mApplicableGameVersion;
            }
        }

        public int InternalResourceVersion
        {
            get
            {
                if (!mIsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return mInternalResourceVersion;
            }
        }

        public Asset[] GetAssets()
        {
            if (!mIsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return mAssets;
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

        public ResourceGroup[] GetResourceGroups()
        {
            if (!mIsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return mResourceGroups;
        }
    }
}
