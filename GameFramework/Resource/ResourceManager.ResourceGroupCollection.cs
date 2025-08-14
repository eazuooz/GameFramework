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
        private sealed class ResourceGroupCollection : IResourceGroupCollection
        {
            private readonly ResourceGroup[] mResourceGroups;
            private readonly Dictionary<ResourceName, ResourceInfo> mResourceInfos;
            private readonly HashSet<ResourceName> mResourceNames;
            private long mTotalLength;
            private long mTotalCompressedLength;

            public ResourceGroupCollection(ResourceGroup[] resourceGroups, Dictionary<ResourceName, ResourceInfo> resourceInfos)
            {
                if (resourceGroups == null || resourceGroups.Length < 1)
                {
                    throw new GameFrameworkException("Resource groups is invalid.");
                }

                if (resourceInfos == null)
                {
                    throw new GameFrameworkException("Resource infos is invalid.");
                }

                int lastIndex = resourceGroups.Length - 1;
                for (int i = 0; i < lastIndex; i++)
                {
                    if (resourceGroups[i] == null)
                    {
                        throw new GameFrameworkException(Utility.Text.Format("Resource group index '{0}' is invalid.", i));
                    }

                    for (int j = i + 1; j < resourceGroups.Length; j++)
                    {
                        if (resourceGroups[i] == resourceGroups[j])
                        {
                            throw new GameFrameworkException(Utility.Text.Format("Resource group '{0}' duplicated.", resourceGroups[i].Name));
                        }
                    }
                }

                if (resourceGroups[lastIndex] == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Resource group index '{0}' is invalid.", lastIndex));
                }

                mResourceGroups = resourceGroups;
                mResourceInfos = resourceInfos;
                mResourceNames = new HashSet<ResourceName>();
                mTotalLength = 0L;
                mTotalCompressedLength = 0L;

                List<ResourceName> cachedResourceNames = new List<ResourceName>();
                foreach (ResourceGroup resourceGroup in mResourceGroups)
                {
                    resourceGroup.InternalGetResourceNames(cachedResourceNames);
                    foreach (ResourceName resourceName in cachedResourceNames)
                    {
                        ResourceInfo resourceInfo = null;
                        if (!mResourceInfos.TryGetValue(resourceName, out resourceInfo))
                        {
                            throw new GameFrameworkException(Utility.Text.Format("Resource info '{0}' is invalid.", resourceName.FullName));
                        }

                        if (mResourceNames.Add(resourceName))
                        {
                            mTotalLength += resourceInfo.Length;
                            mTotalCompressedLength += resourceInfo.CompressedLength;
                        }
                    }
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

            public IResourceGroup[] GetResourceGroups()
            {
                return mResourceGroups;
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
        }
    }
}
