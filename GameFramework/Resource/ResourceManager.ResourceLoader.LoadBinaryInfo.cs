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
        private sealed partial class ResourceLoader
        {
            private sealed class LoadBinaryInfo : IReference
            {
                private string mBinaryAssetName;
                private ResourceInfo mResourceInfo;
                private LoadBinaryCallbacks mLoadBinaryCallbacks;
                private object mUserData;

                public LoadBinaryInfo()
                {
                    mBinaryAssetName = null;
                    mResourceInfo = null;
                    mLoadBinaryCallbacks = null;
                    mUserData = null;
                }

                public string BinaryAssetName
                {
                    get
                    {
                        return mBinaryAssetName;
                    }
                }

                public ResourceInfo ResourceInfo
                {
                    get
                    {
                        return mResourceInfo;
                    }
                }

                public LoadBinaryCallbacks LoadBinaryCallbacks
                {
                    get
                    {
                        return mLoadBinaryCallbacks;
                    }
                }

                public object UserData
                {
                    get
                    {
                        return mUserData;
                    }
                }

                public static LoadBinaryInfo Create(string binaryAssetName, ResourceInfo resourceInfo, LoadBinaryCallbacks loadBinaryCallbacks, object userData)
                {
                    LoadBinaryInfo loadBinaryInfo = ReferencePool.Acquire<LoadBinaryInfo>();
                    loadBinaryInfo.mBinaryAssetName = binaryAssetName;
                    loadBinaryInfo.mResourceInfo = resourceInfo;
                    loadBinaryInfo.mLoadBinaryCallbacks = loadBinaryCallbacks;
                    loadBinaryInfo.mUserData = userData;
                    return loadBinaryInfo;
                }

                public void Clear()
                {
                    mBinaryAssetName = null;
                    mResourceInfo = null;
                    mLoadBinaryCallbacks = null;
                    mUserData = null;
                }
            }
        }
    }
}
