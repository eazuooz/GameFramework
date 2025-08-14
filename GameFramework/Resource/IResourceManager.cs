//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Download;
using GameFramework.FileSystem;
using GameFramework.ObjectPool;
using System;
using System.Collections.Generic;

namespace GameFramework.Resource
{
    public interface IResourceManager
    {
        string ReadOnlyPath
        {
            get;
        }

        string ReadWritePath
        {
            get;
        }

        ResourceMode ResourceMode
        {
            get;
        }

        string CurrentVariant
        {
            get;
        }

        PackageVersionListSerializer PackageVersionListSerializer
        {
            get;
        }

        UpdatableVersionListSerializer UpdatableVersionListSerializer
        {
            get;
        }

        ReadOnlyVersionListSerializer ReadOnlyVersionListSerializer
        {
            get;
        }

        ReadWriteVersionListSerializer ReadWriteVersionListSerializer
        {
            get;
        }

        ResourcePackVersionListSerializer ResourcePackVersionListSerializer
        {
            get;
        }

        string ApplicableGameVersion
        {
            get;
        }

        int InternalResourceVersion
        {
            get;
        }

        int AssetCount
        {
            get;
        }

        int ResourceCount
        {
            get;
        }

        int ResourceGroupCount
        {
            get;
        }

        string UpdatePrefixUri
        {
            get;
            set;
        }

        int GenerateReadWriteVersionListLength
        {
            get;
            set;
        }

        string ApplyingResourcePackPath
        {
            get;
        }

        int ApplyWaitingCount
        {
            get;
        }

        int UpdateRetryCount
        {
            get;
            set;
        }

        IResourceGroup UpdatingResourceGroup
        {
            get;
        }

        int UpdateWaitingCount
        {
            get;
        }

        int UpdateWaitingWhilePlayingCount
        {
            get;
        }

        int UpdateCandidateCount
        {
            get;
        }

        int LoadTotalAgentCount
        {
            get;
        }

        int LoadFreeAgentCount
        {
            get;
        }

        int LoadWorkingAgentCount
        {
            get;
        }

        int LoadWaitingTaskCount
        {
            get;
        }

        float AssetAutoReleaseInterval
        {
            get;
            set;
        }

        int AssetCapacity
        {
            get;
            set;
        }

        float AssetExpireTime
        {
            get;
            set;
        }

        int AssetPriority
        {
            get;
            set;
        }

        float ResourceAutoReleaseInterval
        {
            get;
            set;
        }

        int ResourceCapacity
        {
            get;
            set;
        }

        float ResourceExpireTime
        {
            get;
            set;
        }

        int ResourcePriority
        {
            get;
            set;
        }

        event EventHandler<ResourceVerifyStartEventArgs> ResourceVerifyStart;

        event EventHandler<ResourceVerifySuccessEventArgs> ResourceVerifySuccess;

        event EventHandler<ResourceVerifyFailureEventArgs> ResourceVerifyFailure;

        event EventHandler<ResourceApplyStartEventArgs> ResourceApplyStart;

        event EventHandler<ResourceApplySuccessEventArgs> ResourceApplySuccess;

        event EventHandler<ResourceApplyFailureEventArgs> ResourceApplyFailure;

        event EventHandler<ResourceUpdateStartEventArgs> ResourceUpdateStart;

        event EventHandler<ResourceUpdateChangedEventArgs> ResourceUpdateChanged;

        event EventHandler<ResourceUpdateSuccessEventArgs> ResourceUpdateSuccess;

        event EventHandler<ResourceUpdateFailureEventArgs> ResourceUpdateFailure;

        event EventHandler<ResourceUpdateAllCompleteEventArgs> ResourceUpdateAllComplete;

        void SetReadOnlyPath(string readOnlyPath);

        void SetReadWritePath(string readWritePath);

        void SetResourceMode(ResourceMode resourceMode);

        void SetCurrentVariant(string currentVariant);

        void SetObjectPoolManager(IObjectPoolManager objectPoolManager);

        void SetFileSystemManager(IFileSystemManager fileSystemManager);

        void SetDownloadManager(IDownloadManager downloadManager);

        void SetDecryptResourceCallback(DecryptResourceCallback decryptResourceCallback);

        void SetResourceHelper(IResourceHelper resourceHelper);

        void AddLoadResourceAgentHelper(ILoadResourceAgentHelper loadResourceAgentHelper);

        void InitResources(InitResourcesCompleteCallback initResourcesCompleteCallback);

        CheckVersionListResult CheckVersionList(int latestInternalResourceVersion);

        void UpdateVersionList(int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode, UpdateVersionListCallbacks updateVersionListCallbacks);

        void VerifyResources(int verifyResourceLengthPerFrame, VerifyResourcesCompleteCallback verifyResourcesCompleteCallback);

        void CheckResources(bool ignoreOtherVariant, CheckResourcesCompleteCallback checkResourcesCompleteCallback);

        void ApplyResources(string resourcePackPath, ApplyResourcesCompleteCallback applyResourcesCompleteCallback);

        void UpdateResources(UpdateResourcesCompleteCallback updateResourcesCompleteCallback);

        void UpdateResources(string resourceGroupName, UpdateResourcesCompleteCallback updateResourcesCompleteCallback);

        void StopUpdateResources();

        bool VerifyResourcePack(string resourcePackPath);

        TaskInfo[] GetAllLoadAssetInfos();

        void GetAllLoadAssetInfos(List<TaskInfo> results);

        HasAssetResult HasAsset(string assetName);

        void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks);

        void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks);

        void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks);

        void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData);

        void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks);

        void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData);

        void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData);

        void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData);

        void UnloadAsset(object asset);

        void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks);

        void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks);

        void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData);

        void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData);

        void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks);

        void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData);

        string GetBinaryPath(string binaryAssetName);

        bool GetBinaryPath(string binaryAssetName, out bool storageInReadOnly, out bool storageInFileSystem, out string relativePath, out string fileName);

        int GetBinaryLength(string binaryAssetName);

        void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks);

        void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData);

        byte[] LoadBinaryFromFileSystem(string binaryAssetName);

        int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer);

        int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex);

        int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length);

        byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int length);

        byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, int length);

        int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer);

        int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int length);

        int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length);

        int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer);

        int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int length);

        int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int startIndex, int length);

        bool HasResourceGroup(string resourceGroupName);

        IResourceGroup GetResourceGroup();

        IResourceGroup GetResourceGroup(string resourceGroupName);

        IResourceGroup[] GetAllResourceGroups();

        void GetAllResourceGroups(List<IResourceGroup> results);

        IResourceGroupCollection GetResourceGroupCollection(params string[] resourceGroupNames);

        IResourceGroupCollection GetResourceGroupCollection(List<string> resourceGroupNames);
    }
}
