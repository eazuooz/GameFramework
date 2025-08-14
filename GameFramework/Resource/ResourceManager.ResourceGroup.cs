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
        private sealed class ResourceGroup : IResourceGroup
        {
            private readonly string mName;
            private readonly Dictionary<ResourceName, ResourceInfo> mResourceInfos;
            private readonly HashSet<ResourceName> mResourceNames;
            private long mTotalLength;
            private long mTotalCompressedLength;

            public ResourceGroup(string name, Dictionary<ResourceName, ResourceInfo> resourceInfos)
            {
                if (name == null)
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                if (resourceInfos == null)
                {
                    throw new GameFrameworkException("Resource infos is invalid.");
                }

                mName = name;
                mResourceInfos = resourceInfos;
                mResourceNames = new HashSet<ResourceName>();
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public bool Ready
            {
                get
                {
                    return ReadyCount >= TotalCount;
                }
            }

            public int TotalCount
            {
                get
                {
                    return mResourceNames.Count;
                }
            }

            public int ReadyCount
            {
                get
                {
                    int readyCount = 0;
                    foreach (ResourceName resourceName in mResourceNames)
                    {
                        ResourceInfo resourceInfo = null;
                        if (mResourceInfos.TryGetValue(resourceName, out resourceInfo) && resourceInfo.Ready)
                        {
                            readyCount++;
                        }
                    }

                    return readyCount;
                }
            }

            public long TotalLength
            {
                get
                {
                    return mTotalLength;
                }
            }

            public long TotalCompressedLength
            {
                get
                {
                    return mTotalCompressedLength;
                }
            }

            public long ReadyLength
            {
                get
                {
                    long readyLength = 0L;
                    foreach (ResourceName resourceName in mResourceNames)
                    {
                        ResourceInfo resourceInfo = null;
                        if (mResourceInfos.TryGetValue(resourceName, out resourceInfo) && resourceInfo.Ready)
                        {
                            readyLength += resourceInfo.Length;
                        }
                    }

                    return readyLength;
                }
            }

            public long ReadyCompressedLength
            {
                get
                {
                    long readyCompressedLength = 0L;
                    foreach (ResourceName resourceName in mResourceNames)
                    {
                        ResourceInfo resourceInfo = null;
                        if (mResourceInfos.TryGetValue(resourceName, out resourceInfo) && resourceInfo.Ready)
                        {
                            readyCompressedLength += resourceInfo.CompressedLength;
                        }
                    }

                    return readyCompressedLength;
                }
            }

            public float Progress
            {
                get
                {
                    return mTotalLength > 0L ? (float)ReadyLength / mTotalLength : 1f;
                }
            }

            public string[] GetResourceNames()
            {
                int index = 0;
                string[] resourceNames = new string[mResourceNames.Count];
                foreach (ResourceName resourceName in mResourceNames)
                {
                    resourceNames[index++] = resourceName.FullName;
                }

                return resourceNames;
            }

            public void GetResourceNames(List<string> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (ResourceName resourceName in mResourceNames)
                {
                    results.Add(resourceName.FullName);
                }
            }

            public ResourceName[] InternalGetResourceNames()
            {
                int index = 0;
                ResourceName[] resourceNames = new ResourceName[mResourceNames.Count];
                foreach (ResourceName resourceName in mResourceNames)
                {
                    resourceNames[index++] = resourceName;
                }

                return resourceNames;
            }

            public void InternalGetResourceNames(List<ResourceName> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (ResourceName resourceName in mResourceNames)
                {
                    results.Add(resourceName);
                }
            }

            public bool HasResource(ResourceName resourceName)
            {
                return mResourceNames.Contains(resourceName);
            }

            public void AddResource(ResourceName resourceName, int length, int compressedLength)
            {
                mResourceNames.Add(resourceName);
                mTotalLength += length;
                mTotalCompressedLength += compressedLength;
            }
        }
    }
}
