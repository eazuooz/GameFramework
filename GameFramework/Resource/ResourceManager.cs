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
using System.IO;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private const string RemoteVersionListFileName = "GameFrameworkVersion.dat";
        private const string LocalVersionListFileName = "GameFrameworkList.dat";
        private const string DefaultExtension = "dat";
        private const string TempExtension = "tmp";
        private const int FileSystemMaxFileCount = 1024 * 16;
        private const int FileSystemMaxBlockCount = 1024 * 256;

        private Dictionary<string, AssetInfo> mAssetInfos;
        private Dictionary<ResourceName, ResourceInfo> mResourceInfos;
        private SortedDictionary<ResourceName, ReadWriteResourceInfo> mReadWriteResourceInfos;
        private readonly Dictionary<string, IFileSystem> mReadOnlyFileSystems;
        private readonly Dictionary<string, IFileSystem> mReadWriteFileSystems;
        private readonly Dictionary<string, ResourceGroup> mResourceGroups;

        private PackageVersionListSerializer mPackageVersionListSerializer;
        private UpdatableVersionListSerializer mUpdatableVersionListSerializer;
        private ReadOnlyVersionListSerializer mReadOnlyVersionListSerializer;
        private ReadWriteVersionListSerializer mReadWriteVersionListSerializer;
        private ResourcePackVersionListSerializer mResourcePackVersionListSerializer;

        private IFileSystemManager mFileSystemManager;
        private ResourceIniter mResourceIniter;
        private VersionListProcessor mVersionListProcessor;
        private ResourceVerifier mResourceVerifier;
        private ResourceChecker mResourceChecker;
        private ResourceUpdater mResourceUpdater;
        private ResourceLoader mResourceLoader;
        private IResourceHelper mResourceHelper;

        private string mReadOnlyPath;
        private string mReadWritePath;
        private ResourceMode mResourceMode;
        private bool mRefuseSetFlag;
        private string mCurrentVariant;
        private string mUpdatePrefixUri;
        private string mApplicableGameVersion;
        private int mInternalResourceVersion;
        private MemoryStream mCachedStream;
        private DecryptResourceCallback mDecryptResourceCallback;
        private InitResourcesCompleteCallback mInitResourcesCompleteCallback;
        private UpdateVersionListCallbacks mUpdateVersionListCallbacks;
        private VerifyResourcesCompleteCallback mVerifyResourcesCompleteCallback;
        private CheckResourcesCompleteCallback mCheckResourcesCompleteCallback;
        private ApplyResourcesCompleteCallback mApplyResourcesCompleteCallback;
        private UpdateResourcesCompleteCallback mUpdateResourcesCompleteCallback;
        private EventHandler<ResourceVerifyStartEventArgs> mResourceVerifyStartEventHandler;
        private EventHandler<ResourceVerifySuccessEventArgs> mResourceVerifySuccessEventHandler;
        private EventHandler<ResourceVerifyFailureEventArgs> mResourceVerifyFailureEventHandler;
        private EventHandler<ResourceApplyStartEventArgs> mResourceApplyStartEventHandler;
        private EventHandler<ResourceApplySuccessEventArgs> mResourceApplySuccessEventHandler;
        private EventHandler<ResourceApplyFailureEventArgs> mResourceApplyFailureEventHandler;
        private EventHandler<ResourceUpdateStartEventArgs> mResourceUpdateStartEventHandler;
        private EventHandler<ResourceUpdateChangedEventArgs> mResourceUpdateChangedEventHandler;
        private EventHandler<ResourceUpdateSuccessEventArgs> mResourceUpdateSuccessEventHandler;
        private EventHandler<ResourceUpdateFailureEventArgs> mResourceUpdateFailureEventHandler;
        private EventHandler<ResourceUpdateAllCompleteEventArgs> mResourceUpdateAllCompleteEventHandler;

        public ResourceManager()
        {
            mAssetInfos = null;
            mResourceInfos = null;
            mReadWriteResourceInfos = null;
            mReadOnlyFileSystems = new Dictionary<string, IFileSystem>(StringComparer.Ordinal);
            mReadWriteFileSystems = new Dictionary<string, IFileSystem>(StringComparer.Ordinal);
            mResourceGroups = new Dictionary<string, ResourceGroup>(StringComparer.Ordinal);

            mPackageVersionListSerializer = null;
            mUpdatableVersionListSerializer = null;
            mReadOnlyVersionListSerializer = null;
            mReadWriteVersionListSerializer = null;
            mResourcePackVersionListSerializer = null;

            mResourceIniter = null;
            mVersionListProcessor = null;
            mResourceVerifier = null;
            mResourceChecker = null;
            mResourceUpdater = null;
            mResourceLoader = new ResourceLoader(this);

            mResourceHelper = null;
            mReadOnlyPath = null;
            mReadWritePath = null;
            mResourceMode = ResourceMode.Unspecified;
            mRefuseSetFlag = false;
            mCurrentVariant = null;
            mUpdatePrefixUri = null;
            mApplicableGameVersion = null;
            mInternalResourceVersion = 0;
            mCachedStream = null;
            mDecryptResourceCallback = null;
            mInitResourcesCompleteCallback = null;
            mUpdateVersionListCallbacks = null;
            mVerifyResourcesCompleteCallback = null;
            mCheckResourcesCompleteCallback = null;
            mApplyResourcesCompleteCallback = null;
            mUpdateResourcesCompleteCallback = null;
            mResourceVerifySuccessEventHandler = null;
            mResourceVerifyFailureEventHandler = null;
            mResourceApplySuccessEventHandler = null;
            mResourceApplyFailureEventHandler = null;
            mResourceUpdateStartEventHandler = null;
            mResourceUpdateChangedEventHandler = null;
            mResourceUpdateSuccessEventHandler = null;
            mResourceUpdateFailureEventHandler = null;
            mResourceUpdateAllCompleteEventHandler = null;
        }

        internal override int Priority
        {
            get
            {
                return 3;
            }
        }

        public string ReadOnlyPath
        {
            get
            {
                return mReadOnlyPath;
            }
        }

        public string ReadWritePath
        {
            get
            {
                return mReadWritePath;
            }
        }

        public ResourceMode ResourceMode
        {
            get
            {
                return mResourceMode;
            }
        }

        public string CurrentVariant
        {
            get
            {
                return mCurrentVariant;
            }
        }

        public PackageVersionListSerializer PackageVersionListSerializer
        {
            get
            {
                return mPackageVersionListSerializer;
            }
        }

        public UpdatableVersionListSerializer UpdatableVersionListSerializer
        {
            get
            {
                return mUpdatableVersionListSerializer;
            }
        }

        public ReadOnlyVersionListSerializer ReadOnlyVersionListSerializer
        {
            get
            {
                return mReadOnlyVersionListSerializer;
            }
        }

        public ReadWriteVersionListSerializer ReadWriteVersionListSerializer
        {
            get
            {
                return mReadWriteVersionListSerializer;
            }
        }

        public ResourcePackVersionListSerializer ResourcePackVersionListSerializer
        {
            get
            {
                return mResourcePackVersionListSerializer;
            }
        }

        public string ApplicableGameVersion
        {
            get
            {
                return mApplicableGameVersion;
            }
        }

        public int InternalResourceVersion
        {
            get
            {
                return mInternalResourceVersion;
            }
        }

        public int AssetCount
        {
            get
            {
                return mAssetInfos != null ? mAssetInfos.Count : 0;
            }
        }

        public int ResourceCount
        {
            get
            {
                return mResourceInfos != null ? mResourceInfos.Count : 0;
            }
        }

        public int ResourceGroupCount
        {
            get
            {
                return mResourceGroups.Count;
            }
        }

        public string UpdatePrefixUri
        {
            get
            {
                return mUpdatePrefixUri;
            }
            set
            {
                mUpdatePrefixUri = value;
            }
        }

        public int GenerateReadWriteVersionListLength
        {
            get
            {
                return mResourceUpdater != null ? mResourceUpdater.GenerateReadWriteVersionListLength : 0;
            }
            set
            {
                if (mResourceUpdater == null)
                {
                    throw new GameFrameworkException("You can not use GenerateReadWriteVersionListLength at this time.");
                }

                mResourceUpdater.GenerateReadWriteVersionListLength = value;
            }
        }

        public string ApplyingResourcePackPath
        {
            get
            {
                return mResourceUpdater != null ? mResourceUpdater.ApplyingResourcePackPath : null;
            }
        }

        public int ApplyWaitingCount
        {
            get
            {
                return mResourceUpdater != null ? mResourceUpdater.ApplyWaitingCount : 0;
            }
        }

        public int UpdateRetryCount
        {
            get
            {
                return mResourceUpdater != null ? mResourceUpdater.UpdateRetryCount : 0;
            }
            set
            {
                if (mResourceUpdater == null)
                {
                    throw new GameFrameworkException("You can not use UpdateRetryCount at this time.");
                }

                mResourceUpdater.UpdateRetryCount = value;
            }
        }

        public IResourceGroup UpdatingResourceGroup
        {
            get
            {
                return mResourceUpdater != null ? mResourceUpdater.UpdatingResourceGroup : null;
            }
        }

        public int UpdateWaitingCount
        {
            get
            {
                return mResourceUpdater != null ? mResourceUpdater.UpdateWaitingCount : 0;
            }
        }

        public int UpdateWaitingWhilePlayingCount
        {
            get
            {
                return mResourceUpdater != null ? mResourceUpdater.UpdateWaitingWhilePlayingCount : 0;
            }
        }

        public int UpdateCandidateCount
        {
            get
            {
                return mResourceUpdater != null ? mResourceUpdater.UpdateCandidateCount : 0;
            }
        }

        public int LoadTotalAgentCount
        {
            get
            {
                return mResourceLoader.TotalAgentCount;
            }
        }

        public int LoadFreeAgentCount
        {
            get
            {
                return mResourceLoader.FreeAgentCount;
            }
        }

        public int LoadWorkingAgentCount
        {
            get
            {
                return mResourceLoader.WorkingAgentCount;
            }
        }

        public int LoadWaitingTaskCount
        {
            get
            {
                return mResourceLoader.WaitingTaskCount;
            }
        }

        public float AssetAutoReleaseInterval
        {
            get
            {
                return mResourceLoader.AssetAutoReleaseInterval;
            }
            set
            {
                mResourceLoader.AssetAutoReleaseInterval = value;
            }
        }

        public int AssetCapacity
        {
            get
            {
                return mResourceLoader.AssetCapacity;
            }
            set
            {
                mResourceLoader.AssetCapacity = value;
            }
        }

        public float AssetExpireTime
        {
            get
            {
                return mResourceLoader.AssetExpireTime;
            }
            set
            {
                mResourceLoader.AssetExpireTime = value;
            }
        }

        public int AssetPriority
        {
            get
            {
                return mResourceLoader.AssetPriority;
            }
            set
            {
                mResourceLoader.AssetPriority = value;
            }
        }

        public float ResourceAutoReleaseInterval
        {
            get
            {
                return mResourceLoader.ResourceAutoReleaseInterval;
            }
            set
            {
                mResourceLoader.ResourceAutoReleaseInterval = value;
            }
        }

        public int ResourceCapacity
        {
            get
            {
                return mResourceLoader.ResourceCapacity;
            }
            set
            {
                mResourceLoader.ResourceCapacity = value;
            }
        }

        public float ResourceExpireTime
        {
            get
            {
                return mResourceLoader.ResourceExpireTime;
            }
            set
            {
                mResourceLoader.ResourceExpireTime = value;
            }
        }

        public int ResourcePriority
        {
            get
            {
                return mResourceLoader.ResourcePriority;
            }
            set
            {
                mResourceLoader.ResourcePriority = value;
            }
        }

        public event EventHandler<ResourceVerifyStartEventArgs> ResourceVerifyStart
        {
            add
            {
                mResourceVerifyStartEventHandler += value;
            }
            remove
            {
                mResourceVerifyStartEventHandler -= value;
            }
        }

        public event EventHandler<ResourceVerifySuccessEventArgs> ResourceVerifySuccess
        {
            add
            {
                mResourceVerifySuccessEventHandler += value;
            }
            remove
            {
                mResourceVerifySuccessEventHandler -= value;
            }
        }

        public event EventHandler<ResourceVerifyFailureEventArgs> ResourceVerifyFailure
        {
            add
            {
                mResourceVerifyFailureEventHandler += value;
            }
            remove
            {
                mResourceVerifyFailureEventHandler -= value;
            }
        }

        public event EventHandler<ResourceApplyStartEventArgs> ResourceApplyStart
        {
            add
            {
                mResourceApplyStartEventHandler += value;
            }
            remove
            {
                mResourceApplyStartEventHandler -= value;
            }
        }

        public event EventHandler<ResourceApplySuccessEventArgs> ResourceApplySuccess
        {
            add
            {
                mResourceApplySuccessEventHandler += value;
            }
            remove
            {
                mResourceApplySuccessEventHandler -= value;
            }
        }

        public event EventHandler<ResourceApplyFailureEventArgs> ResourceApplyFailure
        {
            add
            {
                mResourceApplyFailureEventHandler += value;
            }
            remove
            {
                mResourceApplyFailureEventHandler -= value;
            }
        }

        public event EventHandler<ResourceUpdateStartEventArgs> ResourceUpdateStart
        {
            add
            {
                mResourceUpdateStartEventHandler += value;
            }
            remove
            {
                mResourceUpdateStartEventHandler -= value;
            }
        }

        public event EventHandler<ResourceUpdateChangedEventArgs> ResourceUpdateChanged
        {
            add
            {
                mResourceUpdateChangedEventHandler += value;
            }
            remove
            {
                mResourceUpdateChangedEventHandler -= value;
            }
        }

        public event EventHandler<ResourceUpdateSuccessEventArgs> ResourceUpdateSuccess
        {
            add
            {
                mResourceUpdateSuccessEventHandler += value;
            }
            remove
            {
                mResourceUpdateSuccessEventHandler -= value;
            }
        }

        public event EventHandler<ResourceUpdateFailureEventArgs> ResourceUpdateFailure
        {
            add
            {
                mResourceUpdateFailureEventHandler += value;
            }
            remove
            {
                mResourceUpdateFailureEventHandler -= value;
            }
        }

        public event EventHandler<ResourceUpdateAllCompleteEventArgs> ResourceUpdateAllComplete
        {
            add
            {
                mResourceUpdateAllCompleteEventHandler += value;
            }
            remove
            {
                mResourceUpdateAllCompleteEventHandler -= value;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (mResourceVerifier != null)
            {
                mResourceVerifier.Update(elapseSeconds, realElapseSeconds);
                return;
            }

            if (mResourceUpdater != null)
            {
                mResourceUpdater.Update(elapseSeconds, realElapseSeconds);
            }

            mResourceLoader.Update(elapseSeconds, realElapseSeconds);
        }

        internal override void Shutdown()
        {
            if (mResourceIniter != null)
            {
                mResourceIniter.Shutdown();
                mResourceIniter = null;
            }

            if (mVersionListProcessor != null)
            {
                mVersionListProcessor.VersionListUpdateSuccess -= OnVersionListProcessorUpdateSuccess;
                mVersionListProcessor.VersionListUpdateFailure -= OnVersionListProcessorUpdateFailure;
                mVersionListProcessor.Shutdown();
                mVersionListProcessor = null;
            }

            if (mResourceVerifier != null)
            {
                mResourceVerifier.ResourceVerifyStart -= OnVerifierResourceVerifyStart;
                mResourceVerifier.ResourceVerifySuccess -= OnVerifierResourceVerifySuccess;
                mResourceVerifier.ResourceVerifyFailure -= OnVerifierResourceVerifyFailure;
                mResourceVerifier.ResourceVerifyComplete -= OnVerifierResourceVerifyComplete;
                mResourceVerifier.Shutdown();
                mResourceVerifier = null;
            }

            if (mResourceChecker != null)
            {
                mResourceChecker.ResourceNeedUpdate -= OnCheckerResourceNeedUpdate;
                mResourceChecker.ResourceCheckComplete -= OnCheckerResourceCheckComplete;
                mResourceChecker.Shutdown();
                mResourceChecker = null;
            }

            if (mResourceUpdater != null)
            {
                mResourceUpdater.ResourceApplyStart -= OnUpdaterResourceApplyStart;
                mResourceUpdater.ResourceApplySuccess -= OnUpdaterResourceApplySuccess;
                mResourceUpdater.ResourceApplyFailure -= OnUpdaterResourceApplyFailure;
                mResourceUpdater.ResourceApplyComplete -= OnUpdaterResourceApplyComplete;
                mResourceUpdater.ResourceUpdateStart -= OnUpdaterResourceUpdateStart;
                mResourceUpdater.ResourceUpdateChanged -= OnUpdaterResourceUpdateChanged;
                mResourceUpdater.ResourceUpdateSuccess -= OnUpdaterResourceUpdateSuccess;
                mResourceUpdater.ResourceUpdateFailure -= OnUpdaterResourceUpdateFailure;
                mResourceUpdater.ResourceUpdateComplete -= OnUpdaterResourceUpdateComplete;
                mResourceUpdater.ResourceUpdateAllComplete -= OnUpdaterResourceUpdateAllComplete;
                mResourceUpdater.Shutdown();
                mResourceUpdater = null;

                if (mReadWriteResourceInfos != null)
                {
                    mReadWriteResourceInfos.Clear();
                    mReadWriteResourceInfos = null;
                }

                FreeCachedStream();
            }

            if (mResourceLoader != null)
            {
                mResourceLoader.Shutdown();
                mResourceLoader = null;
            }

            if (mAssetInfos != null)
            {
                mAssetInfos.Clear();
                mAssetInfos = null;
            }

            if (mResourceInfos != null)
            {
                mResourceInfos.Clear();
                mResourceInfos = null;
            }

            mReadOnlyFileSystems.Clear();
            mReadWriteFileSystems.Clear();
            mResourceGroups.Clear();
        }

        public void SetReadOnlyPath(string readOnlyPath)
        {
            if (string.IsNullOrEmpty(readOnlyPath))
            {
                throw new GameFrameworkException("Read-only path is invalid.");
            }

            if (mRefuseSetFlag)
            {
                throw new GameFrameworkException("You can not set read-only path at this time.");
            }

            if (mResourceLoader.TotalAgentCount > 0)
            {
                throw new GameFrameworkException("You must set read-only path before add load resource agent helper.");
            }

            mReadOnlyPath = readOnlyPath;
        }

        public void SetReadWritePath(string readWritePath)
        {
            if (string.IsNullOrEmpty(readWritePath))
            {
                throw new GameFrameworkException("Read-write path is invalid.");
            }

            if (mRefuseSetFlag)
            {
                throw new GameFrameworkException("You can not set read-write path at this time.");
            }

            if (mResourceLoader.TotalAgentCount > 0)
            {
                throw new GameFrameworkException("You must set read-write path before add load resource agent helper.");
            }

            mReadWritePath = readWritePath;
        }

        public void SetResourceMode(ResourceMode resourceMode)
        {
            if (resourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("Resource mode is invalid.");
            }

            if (mRefuseSetFlag)
            {
                throw new GameFrameworkException("You can not set resource mode at this time.");
            }

            if (mResourceMode == ResourceMode.Unspecified)
            {
                mResourceMode = resourceMode;

                if (mResourceMode == ResourceMode.Package)
                {
                    mPackageVersionListSerializer = new PackageVersionListSerializer();

                    mResourceIniter = new ResourceIniter(this);
                    mResourceIniter.ResourceInitComplete += OnIniterResourceInitComplete;
                }
                else if (mResourceMode == ResourceMode.Updatable || mResourceMode == ResourceMode.UpdatableWhilePlaying)
                {
                    mUpdatableVersionListSerializer = new UpdatableVersionListSerializer();
                    mReadOnlyVersionListSerializer = new ReadOnlyVersionListSerializer();
                    mReadWriteVersionListSerializer = new ReadWriteVersionListSerializer();
                    mResourcePackVersionListSerializer = new ResourcePackVersionListSerializer();

                    mVersionListProcessor = new VersionListProcessor(this);
                    mVersionListProcessor.VersionListUpdateSuccess += OnVersionListProcessorUpdateSuccess;
                    mVersionListProcessor.VersionListUpdateFailure += OnVersionListProcessorUpdateFailure;

                    mResourceChecker = new ResourceChecker(this);
                    mResourceChecker.ResourceNeedUpdate += OnCheckerResourceNeedUpdate;
                    mResourceChecker.ResourceCheckComplete += OnCheckerResourceCheckComplete;

                    mResourceUpdater = new ResourceUpdater(this);
                    mResourceUpdater.ResourceApplyStart += OnUpdaterResourceApplyStart;
                    mResourceUpdater.ResourceApplySuccess += OnUpdaterResourceApplySuccess;
                    mResourceUpdater.ResourceApplyFailure += OnUpdaterResourceApplyFailure;
                    mResourceUpdater.ResourceApplyComplete += OnUpdaterResourceApplyComplete;
                    mResourceUpdater.ResourceUpdateStart += OnUpdaterResourceUpdateStart;
                    mResourceUpdater.ResourceUpdateChanged += OnUpdaterResourceUpdateChanged;
                    mResourceUpdater.ResourceUpdateSuccess += OnUpdaterResourceUpdateSuccess;
                    mResourceUpdater.ResourceUpdateFailure += OnUpdaterResourceUpdateFailure;
                    mResourceUpdater.ResourceUpdateComplete += OnUpdaterResourceUpdateComplete;
                    mResourceUpdater.ResourceUpdateAllComplete += OnUpdaterResourceUpdateAllComplete;
                }
            }
            else if (mResourceMode != resourceMode)
            {
                throw new GameFrameworkException("You can not change resource mode at this time.");
            }
        }

        public void SetCurrentVariant(string currentVariant)
        {
            if (mRefuseSetFlag)
            {
                throw new GameFrameworkException("You can not set current variant at this time.");
            }

            mCurrentVariant = currentVariant;
        }

        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new GameFrameworkException("Object pool manager is invalid.");
            }

            mResourceLoader.SetObjectPoolManager(objectPoolManager);
        }

        public void SetFileSystemManager(IFileSystemManager fileSystemManager)
        {
            if (fileSystemManager == null)
            {
                throw new GameFrameworkException("File system manager is invalid.");
            }

            mFileSystemManager = fileSystemManager;
        }

        public void SetDownloadManager(IDownloadManager downloadManager)
        {
            if (downloadManager == null)
            {
                throw new GameFrameworkException("Download manager is invalid.");
            }

            if (mVersionListProcessor != null)
            {
                mVersionListProcessor.SetDownloadManager(downloadManager);
            }

            if (mResourceUpdater != null)
            {
                mResourceUpdater.SetDownloadManager(downloadManager);
            }
        }

        public void SetDecryptResourceCallback(DecryptResourceCallback decryptResourceCallback)
        {
            if (mResourceLoader.TotalAgentCount > 0)
            {
                throw new GameFrameworkException("You must set decrypt resource callback before add load resource agent helper.");
            }

            mDecryptResourceCallback = decryptResourceCallback;
        }

        public void SetResourceHelper(IResourceHelper resourceHelper)
        {
            if (resourceHelper == null)
            {
                throw new GameFrameworkException("Resource helper is invalid.");
            }

            if (mResourceLoader.TotalAgentCount > 0)
            {
                throw new GameFrameworkException("You must set resource helper before add load resource agent helper.");
            }

            mResourceHelper = resourceHelper;
        }

        public void AddLoadResourceAgentHelper(ILoadResourceAgentHelper loadResourceAgentHelper)
        {
            if (mResourceHelper == null)
            {
                throw new GameFrameworkException("Resource helper is invalid.");
            }

            if (string.IsNullOrEmpty(mReadOnlyPath))
            {
                throw new GameFrameworkException("Read-only path is invalid.");
            }

            if (string.IsNullOrEmpty(mReadWritePath))
            {
                throw new GameFrameworkException("Read-write path is invalid.");
            }

            mResourceLoader.AddLoadResourceAgentHelper(loadResourceAgentHelper, mResourceHelper, mReadOnlyPath, mReadWritePath, mDecryptResourceCallback);
        }

        public void InitResources(InitResourcesCompleteCallback initResourcesCompleteCallback)
        {
            if (initResourcesCompleteCallback == null)
            {
                throw new GameFrameworkException("Init resources complete callback is invalid.");
            }

            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Package)
            {
                throw new GameFrameworkException("You can not use InitResources without package resource mode.");
            }

            if (mResourceIniter == null)
            {
                throw new GameFrameworkException("You can not use InitResources at this time.");
            }

            mRefuseSetFlag = true;
            mInitResourcesCompleteCallback = initResourcesCompleteCallback;
            mResourceIniter.InitResources(mCurrentVariant);
        }

        public CheckVersionListResult CheckVersionList(int latestInternalResourceVersion)
        {
            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Updatable && mResourceMode != ResourceMode.UpdatableWhilePlaying)
            {
                throw new GameFrameworkException("You can not use CheckVersionList without updatable resource mode.");
            }

            if (mVersionListProcessor == null)
            {
                throw new GameFrameworkException("You can not use CheckVersionList at this time.");
            }

            return mVersionListProcessor.CheckVersionList(latestInternalResourceVersion);
        }

        public void UpdateVersionList(int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode, UpdateVersionListCallbacks updateVersionListCallbacks)
        {
            if (updateVersionListCallbacks == null)
            {
                throw new GameFrameworkException("Update version list callbacks is invalid.");
            }

            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Updatable && mResourceMode != ResourceMode.UpdatableWhilePlaying)
            {
                throw new GameFrameworkException("You can not use UpdateVersionList without updatable resource mode.");
            }

            if (mVersionListProcessor == null)
            {
                throw new GameFrameworkException("You can not use UpdateVersionList at this time.");
            }

            mUpdateVersionListCallbacks = updateVersionListCallbacks;
            mVersionListProcessor.UpdateVersionList(versionListLength, versionListHashCode, versionListCompressedLength, versionListCompressedHashCode);
        }

        public void VerifyResources(int verifyResourceLengthPerFrame, VerifyResourcesCompleteCallback verifyResourcesCompleteCallback)
        {
            if (verifyResourcesCompleteCallback == null)
            {
                throw new GameFrameworkException("Verify resources complete callback is invalid.");
            }

            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Updatable && mResourceMode != ResourceMode.UpdatableWhilePlaying)
            {
                throw new GameFrameworkException("You can not use VerifyResources without updatable resource mode.");
            }

            if (mRefuseSetFlag)
            {
                throw new GameFrameworkException("You can not verify resources at this time.");
            }

            mResourceVerifier = new ResourceVerifier(this);
            mResourceVerifier.ResourceVerifyStart += OnVerifierResourceVerifyStart;
            mResourceVerifier.ResourceVerifySuccess += OnVerifierResourceVerifySuccess;
            mResourceVerifier.ResourceVerifyFailure += OnVerifierResourceVerifyFailure;
            mResourceVerifier.ResourceVerifyComplete += OnVerifierResourceVerifyComplete;
            mVerifyResourcesCompleteCallback = verifyResourcesCompleteCallback;
            mResourceVerifier.VerifyResources(verifyResourceLengthPerFrame);
        }

        public void CheckResources(bool ignoreOtherVariant, CheckResourcesCompleteCallback checkResourcesCompleteCallback)
        {
            if (checkResourcesCompleteCallback == null)
            {
                throw new GameFrameworkException("Check resources complete callback is invalid.");
            }

            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Updatable && mResourceMode != ResourceMode.UpdatableWhilePlaying)
            {
                throw new GameFrameworkException("You can not use CheckResources without updatable resource mode.");
            }

            if (mResourceChecker == null)
            {
                throw new GameFrameworkException("You can not use CheckResources at this time.");
            }

            mRefuseSetFlag = true;
            mCheckResourcesCompleteCallback = checkResourcesCompleteCallback;
            mResourceChecker.CheckResources(mCurrentVariant, ignoreOtherVariant);
        }

        public void ApplyResources(string resourcePackPath, ApplyResourcesCompleteCallback applyResourcesCompleteCallback)
        {
            if (string.IsNullOrEmpty(resourcePackPath))
            {
                throw new GameFrameworkException("Resource pack path is invalid.");
            }

            if (!File.Exists(resourcePackPath))
            {
                throw new GameFrameworkException(Utility.Text.Format("Resource pack '{0}' is not exist.", resourcePackPath));
            }

            if (applyResourcesCompleteCallback == null)
            {
                throw new GameFrameworkException("Apply resources complete callback is invalid.");
            }

            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Updatable && mResourceMode != ResourceMode.UpdatableWhilePlaying)
            {
                throw new GameFrameworkException("You can not use ApplyResources without updatable resource mode.");
            }

            if (mResourceUpdater == null)
            {
                throw new GameFrameworkException("You can not use ApplyResources at this time.");
            }

            mApplyResourcesCompleteCallback = applyResourcesCompleteCallback;
            mResourceUpdater.ApplyResources(resourcePackPath);
        }

        public void UpdateResources(UpdateResourcesCompleteCallback updateResourcesCompleteCallback)
        {
            UpdateResources(string.Empty, updateResourcesCompleteCallback);
        }

        public void UpdateResources(string resourceGroupName, UpdateResourcesCompleteCallback updateResourcesCompleteCallback)
        {
            if (updateResourcesCompleteCallback == null)
            {
                throw new GameFrameworkException("Update resources complete callback is invalid.");
            }

            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Updatable && mResourceMode != ResourceMode.UpdatableWhilePlaying)
            {
                throw new GameFrameworkException("You can not use UpdateResources without updatable resource mode.");
            }

            if (mResourceUpdater == null)
            {
                throw new GameFrameworkException("You can not use UpdateResources at this time.");
            }

            ResourceGroup resourceGroup = (ResourceGroup)GetResourceGroup(resourceGroupName);
            if (resourceGroup == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find resource group '{0}'.", resourceGroupName));
            }

            mUpdateResourcesCompleteCallback = updateResourcesCompleteCallback;
            mResourceUpdater.UpdateResources(resourceGroup);
        }

        public void StopUpdateResources()
        {
            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Updatable && mResourceMode != ResourceMode.UpdatableWhilePlaying)
            {
                throw new GameFrameworkException("You can not use StopUpdateResources without updatable resource mode.");
            }

            if (mResourceUpdater == null)
            {
                throw new GameFrameworkException("You can not use StopUpdateResources at this time.");
            }

            mResourceUpdater.StopUpdateResources();
            mUpdateResourcesCompleteCallback = null;
        }

        public bool VerifyResourcePack(string resourcePackPath)
        {
            if (string.IsNullOrEmpty(resourcePackPath))
            {
                throw new GameFrameworkException("Resource pack path is invalid.");
            }

            if (!File.Exists(resourcePackPath))
            {
                throw new GameFrameworkException(Utility.Text.Format("Resource pack '{0}' is not exist.", resourcePackPath));
            }

            if (mResourceMode == ResourceMode.Unspecified)
            {
                throw new GameFrameworkException("You must set resource mode first.");
            }

            if (mResourceMode != ResourceMode.Updatable && mResourceMode != ResourceMode.UpdatableWhilePlaying)
            {
                throw new GameFrameworkException("You can not use VerifyResourcePack without updatable resource mode.");
            }

            if (mResourcePackVersionListSerializer == null)
            {
                throw new GameFrameworkException("You can not use VerifyResourcePack at this time.");
            }

            try
            {
                long length = 0L;
                ResourcePackVersionList versionList = default(ResourcePackVersionList);
                using (FileStream fileStream = new FileStream(resourcePackPath, FileMode.Open, FileAccess.Read))
                {
                    length = fileStream.Length;
                    versionList = mResourcePackVersionListSerializer.Deserialize(fileStream);
                }

                if (!versionList.IsValid)
                {
                    return false;
                }

                if (versionList.Offset + versionList.Length != length)
                {
                    return false;
                }

                int hashCode = 0;
                using (FileStream fileStream = new FileStream(resourcePackPath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Position = versionList.Offset;
                    hashCode = Utility.Verifier.GetCrc32(fileStream);
                }

                if (versionList.HashCode != hashCode)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public TaskInfo[] GetAllLoadAssetInfos()
        {
            return mResourceLoader.GetAllLoadAssetInfos();
        }

        public void GetAllLoadAssetInfos(List<TaskInfo> results)
        {
            mResourceLoader.GetAllLoadAssetInfos(results);
        }

        public HasAssetResult HasAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            return mResourceLoader.HasAsset(assetName);
        }

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            mResourceLoader.LoadAsset(assetName, null, Constant.DefaultPriority, loadAssetCallbacks, null);
        }

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            mResourceLoader.LoadAsset(assetName, assetType, Constant.DefaultPriority, loadAssetCallbacks, null);
        }

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            mResourceLoader.LoadAsset(assetName, null, priority, loadAssetCallbacks, null);
        }

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            mResourceLoader.LoadAsset(assetName, null, Constant.DefaultPriority, loadAssetCallbacks, userData);
        }

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            mResourceLoader.LoadAsset(assetName, assetType, priority, loadAssetCallbacks, null);
        }

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            mResourceLoader.LoadAsset(assetName, assetType, Constant.DefaultPriority, loadAssetCallbacks, userData);
        }

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            mResourceLoader.LoadAsset(assetName, null, priority, loadAssetCallbacks, userData);
        }

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            mResourceLoader.LoadAsset(assetName, assetType, priority, loadAssetCallbacks, userData);
        }

        public void UnloadAsset(object asset)
        {
            if (asset == null)
            {
                throw new GameFrameworkException("Asset is invalid.");
            }

            if (mResourceLoader == null)
            {
                return;
            }

            mResourceLoader.UnloadAsset(asset);
        }

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            mResourceLoader.LoadScene(sceneAssetName, Constant.DefaultPriority, loadSceneCallbacks, null);
        }

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            mResourceLoader.LoadScene(sceneAssetName, priority, loadSceneCallbacks, null);
        }

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            mResourceLoader.LoadScene(sceneAssetName, Constant.DefaultPriority, loadSceneCallbacks, userData);
        }

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            mResourceLoader.LoadScene(sceneAssetName, priority, loadSceneCallbacks, userData);
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (unloadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Unload scene callbacks is invalid.");
            }

            mResourceLoader.UnloadScene(sceneAssetName, unloadSceneCallbacks, null);
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (unloadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Unload scene callbacks is invalid.");
            }

            mResourceLoader.UnloadScene(sceneAssetName, unloadSceneCallbacks, userData);
        }

        public string GetBinaryPath(string binaryAssetName)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            return mResourceLoader.GetBinaryPath(binaryAssetName);
        }

        public bool GetBinaryPath(string binaryAssetName, out bool storageInReadOnly, out bool storageInFileSystem, out string relativePath, out string fileName)
        {
            return mResourceLoader.GetBinaryPath(binaryAssetName, out storageInReadOnly, out storageInFileSystem, out relativePath, out fileName);
        }

        public int GetBinaryLength(string binaryAssetName)
        {
            return mResourceLoader.GetBinaryLength(binaryAssetName);
        }

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (loadBinaryCallbacks == null)
            {
                throw new GameFrameworkException("Load binary callbacks is invalid.");
            }

            mResourceLoader.LoadBinary(binaryAssetName, loadBinaryCallbacks, null);
        }

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (loadBinaryCallbacks == null)
            {
                throw new GameFrameworkException("Load binary callbacks is invalid.");
            }

            mResourceLoader.LoadBinary(binaryAssetName, loadBinaryCallbacks, userData);
        }

        public byte[] LoadBinaryFromFileSystem(string binaryAssetName)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            return mResourceLoader.LoadBinaryFromFileSystem(binaryAssetName);
        }

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinaryFromFileSystem(binaryAssetName, buffer, 0, buffer.Length);
        }

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinaryFromFileSystem(binaryAssetName, buffer, startIndex, buffer.Length - startIndex);
        }

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinaryFromFileSystem(binaryAssetName, buffer, startIndex, length);
        }

        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            return mResourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, 0, length);
        }

        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            return mResourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, offset, length);
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, 0, buffer.Length);
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, 0, length);
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, 0, buffer, startIndex, length);
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, offset, buffer, 0, buffer.Length);
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, offset, buffer, 0, length);
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(binaryAssetName))
            {
                throw new GameFrameworkException("Binary asset name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return mResourceLoader.LoadBinarySegmentFromFileSystem(binaryAssetName, offset, buffer, startIndex, length);
        }

        public bool HasResourceGroup(string resourceGroupName)
        {
            return mResourceGroups.ContainsKey(resourceGroupName ?? string.Empty);
        }

        public IResourceGroup GetResourceGroup()
        {
            return GetResourceGroup(string.Empty);
        }

        public IResourceGroup GetResourceGroup(string resourceGroupName)
        {
            ResourceGroup resourceGroup = null;
            if (mResourceGroups.TryGetValue(resourceGroupName ?? string.Empty, out resourceGroup))
            {
                return resourceGroup;
            }

            return null;
        }

        public IResourceGroup[] GetAllResourceGroups()
        {
            int index = 0;
            IResourceGroup[] results = new IResourceGroup[mResourceGroups.Count];
            foreach (KeyValuePair<string, ResourceGroup> resourceGroup in mResourceGroups)
            {
                results[index++] = resourceGroup.Value;
            }

            return results;
        }

        public void GetAllResourceGroups(List<IResourceGroup> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, ResourceGroup> resourceGroup in mResourceGroups)
            {
                results.Add(resourceGroup.Value);
            }
        }

        public IResourceGroupCollection GetResourceGroupCollection(params string[] resourceGroupNames)
        {
            if (resourceGroupNames == null || resourceGroupNames.Length < 1)
            {
                throw new GameFrameworkException("Resource group names is invalid.");
            }

            ResourceGroup[] resourceGroups = new ResourceGroup[resourceGroupNames.Length];
            for (int i = 0; i < resourceGroupNames.Length; i++)
            {
                if (string.IsNullOrEmpty(resourceGroupNames[i]))
                {
                    throw new GameFrameworkException("Resource group name is invalid.");
                }

                resourceGroups[i] = (ResourceGroup)GetResourceGroup(resourceGroupNames[i]);
                if (resourceGroups[i] == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Resource group '{0}' is not exist.", resourceGroupNames[i]));
                }
            }

            return new ResourceGroupCollection(resourceGroups, mResourceInfos);
        }

        public IResourceGroupCollection GetResourceGroupCollection(List<string> resourceGroupNames)
        {
            if (resourceGroupNames == null || resourceGroupNames.Count < 1)
            {
                throw new GameFrameworkException("Resource group names is invalid.");
            }

            ResourceGroup[] resourceGroups = new ResourceGroup[resourceGroupNames.Count];
            for (int i = 0; i < resourceGroupNames.Count; i++)
            {
                if (string.IsNullOrEmpty(resourceGroupNames[i]))
                {
                    throw new GameFrameworkException("Resource group name is invalid.");
                }

                resourceGroups[i] = (ResourceGroup)GetResourceGroup(resourceGroupNames[i]);
                if (resourceGroups[i] == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Resource group '{0}' is not exist.", resourceGroupNames[i]));
                }
            }

            return new ResourceGroupCollection(resourceGroups, mResourceInfos);
        }

        private void UpdateResource(ResourceName resourceName)
        {
            mResourceUpdater.UpdateResource(resourceName);
        }

        private ResourceGroup GetOrAddResourceGroup(string resourceGroupName)
        {
            if (resourceGroupName == null)
            {
                resourceGroupName = string.Empty;
            }

            ResourceGroup resourceGroup = null;
            if (!mResourceGroups.TryGetValue(resourceGroupName, out resourceGroup))
            {
                resourceGroup = new ResourceGroup(resourceGroupName, mResourceInfos);
                mResourceGroups.Add(resourceGroupName, resourceGroup);
            }

            return resourceGroup;
        }

        private AssetInfo GetAssetInfo(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (mAssetInfos == null)
            {
                return null;
            }

            AssetInfo assetInfo = null;
            if (mAssetInfos.TryGetValue(assetName, out assetInfo))
            {
                return assetInfo;
            }

            return null;
        }

        private ResourceInfo GetResourceInfo(ResourceName resourceName)
        {
            if (mResourceInfos == null)
            {
                return null;
            }

            ResourceInfo resourceInfo = null;
            if (mResourceInfos.TryGetValue(resourceName, out resourceInfo))
            {
                return resourceInfo;
            }

            return null;
        }

        private IFileSystem GetFileSystem(string fileSystemName, bool storageInReadOnly)
        {
            if (string.IsNullOrEmpty(fileSystemName))
            {
                throw new GameFrameworkException("File system name is invalid.");
            }

            IFileSystem fileSystem = null;
            if (storageInReadOnly)
            {
                if (!mReadOnlyFileSystems.TryGetValue(fileSystemName, out fileSystem))
                {
                    string fullPath = Utility.Path.GetRegularPath(Path.Combine(mReadOnlyPath, Utility.Text.Format("{0}.{1}", fileSystemName, DefaultExtension)));
                    fileSystem = mFileSystemManager.GetFileSystem(fullPath);
                    if (fileSystem == null)
                    {
                        fileSystem = mFileSystemManager.LoadFileSystem(fullPath, FileSystemAccess.Read);
                        mReadOnlyFileSystems.Add(fileSystemName, fileSystem);
                    }
                }
            }
            else
            {
                if (!mReadWriteFileSystems.TryGetValue(fileSystemName, out fileSystem))
                {
                    string fullPath = Utility.Path.GetRegularPath(Path.Combine(mReadWritePath, Utility.Text.Format("{0}.{1}", fileSystemName, DefaultExtension)));
                    fileSystem = mFileSystemManager.GetFileSystem(fullPath);
                    if (fileSystem == null)
                    {
                        if (File.Exists(fullPath))
                        {
                            fileSystem = mFileSystemManager.LoadFileSystem(fullPath, FileSystemAccess.ReadWrite);
                        }
                        else
                        {
                            string directory = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            fileSystem = mFileSystemManager.CreateFileSystem(fullPath, FileSystemAccess.ReadWrite, FileSystemMaxFileCount, FileSystemMaxBlockCount);
                        }

                        mReadWriteFileSystems.Add(fileSystemName, fileSystem);
                    }
                }
            }

            return fileSystem;
        }

        private void PrepareCachedStream()
        {
            if (mCachedStream == null)
            {
                mCachedStream = new MemoryStream();
            }

            mCachedStream.Position = 0L;
            mCachedStream.SetLength(0L);
        }

        private void FreeCachedStream()
        {
            if (mCachedStream != null)
            {
                mCachedStream.Dispose();
                mCachedStream = null;
            }
        }

        private void OnIniterResourceInitComplete()
        {
            mResourceIniter.ResourceInitComplete -= OnIniterResourceInitComplete;
            mResourceIniter.Shutdown();
            mResourceIniter = null;

            mInitResourcesCompleteCallback();
            mInitResourcesCompleteCallback = null;
        }

        private void OnVersionListProcessorUpdateSuccess(string downloadPath, string downloadUri)
        {
            mUpdateVersionListCallbacks.UpdateVersionListSuccessCallback(downloadPath, downloadUri);
        }

        private void OnVersionListProcessorUpdateFailure(string downloadUri, string errorMessage)
        {
            if (mUpdateVersionListCallbacks.UpdateVersionListFailureCallback != null)
            {
                mUpdateVersionListCallbacks.UpdateVersionListFailureCallback(downloadUri, errorMessage);
            }
        }

        private void OnVerifierResourceVerifyStart(int count, long totalLength)
        {
            if (mResourceVerifyStartEventHandler != null)
            {
                ResourceVerifyStartEventArgs resourceVerifyStartEventArgs = ResourceVerifyStartEventArgs.Create(count, totalLength);
                mResourceVerifyStartEventHandler(this, resourceVerifyStartEventArgs);
                ReferencePool.Release(resourceVerifyStartEventArgs);
            }
        }

        private void OnVerifierResourceVerifySuccess(ResourceName resourceName, int length)
        {
            if (mResourceVerifySuccessEventHandler != null)
            {
                ResourceVerifySuccessEventArgs resourceVerifySuccessEventArgs = ResourceVerifySuccessEventArgs.Create(resourceName.FullName, length);
                mResourceVerifySuccessEventHandler(this, resourceVerifySuccessEventArgs);
                ReferencePool.Release(resourceVerifySuccessEventArgs);
            }
        }

        private void OnVerifierResourceVerifyFailure(ResourceName resourceName)
        {
            if (mResourceVerifyFailureEventHandler != null)
            {
                ResourceVerifyFailureEventArgs resourceVerifyFailureEventArgs = ResourceVerifyFailureEventArgs.Create(resourceName.FullName);
                mResourceVerifyFailureEventHandler(this, resourceVerifyFailureEventArgs);
                ReferencePool.Release(resourceVerifyFailureEventArgs);
            }
        }

        private void OnVerifierResourceVerifyComplete(bool result)
        {
            mVerifyResourcesCompleteCallback(result);
            mResourceVerifier.ResourceVerifyStart -= OnVerifierResourceVerifyStart;
            mResourceVerifier.ResourceVerifySuccess -= OnVerifierResourceVerifySuccess;
            mResourceVerifier.ResourceVerifyFailure -= OnVerifierResourceVerifyFailure;
            mResourceVerifier.ResourceVerifyComplete -= OnVerifierResourceVerifyComplete;
            mResourceVerifier.Shutdown();
            mResourceVerifier = null;
        }

        private void OnCheckerResourceNeedUpdate(ResourceName resourceName, string fileSystemName, LoadType loadType, int length, int hashCode, int compressedLength, int compressedHashCode)
        {
            mResourceUpdater.AddResourceUpdate(resourceName, fileSystemName, loadType, length, hashCode, compressedLength, compressedHashCode, Utility.Path.GetRegularPath(Path.Combine(mReadWritePath, resourceName.FullName)));
        }

        private void OnCheckerResourceCheckComplete(int movedCount, int removedCount, int updateCount, long updateTotalLength, long updateTotalCompressedLength)
        {
            mVersionListProcessor.VersionListUpdateSuccess -= OnVersionListProcessorUpdateSuccess;
            mVersionListProcessor.VersionListUpdateFailure -= OnVersionListProcessorUpdateFailure;
            mVersionListProcessor.Shutdown();
            mVersionListProcessor = null;
            mUpdateVersionListCallbacks = null;

            mResourceChecker.ResourceNeedUpdate -= OnCheckerResourceNeedUpdate;
            mResourceChecker.ResourceCheckComplete -= OnCheckerResourceCheckComplete;
            mResourceChecker.Shutdown();
            mResourceChecker = null;

            mResourceUpdater.CheckResourceComplete(movedCount > 0 || removedCount > 0);

            if (updateCount <= 0)
            {
                mResourceUpdater.ResourceApplyStart -= OnUpdaterResourceApplyStart;
                mResourceUpdater.ResourceApplySuccess -= OnUpdaterResourceApplySuccess;
                mResourceUpdater.ResourceApplyFailure -= OnUpdaterResourceApplyFailure;
                mResourceUpdater.ResourceApplyComplete -= OnUpdaterResourceApplyComplete;
                mResourceUpdater.ResourceUpdateStart -= OnUpdaterResourceUpdateStart;
                mResourceUpdater.ResourceUpdateChanged -= OnUpdaterResourceUpdateChanged;
                mResourceUpdater.ResourceUpdateSuccess -= OnUpdaterResourceUpdateSuccess;
                mResourceUpdater.ResourceUpdateFailure -= OnUpdaterResourceUpdateFailure;
                mResourceUpdater.ResourceUpdateComplete -= OnUpdaterResourceUpdateComplete;
                mResourceUpdater.ResourceUpdateAllComplete -= OnUpdaterResourceUpdateAllComplete;
                mResourceUpdater.Shutdown();
                mResourceUpdater = null;

                mReadWriteResourceInfos.Clear();
                mReadWriteResourceInfos = null;

                FreeCachedStream();
            }

            mCheckResourcesCompleteCallback(movedCount, removedCount, updateCount, updateTotalLength, updateTotalCompressedLength);
            mCheckResourcesCompleteCallback = null;
        }

        private void OnUpdaterResourceApplyStart(string resourcePackPath, int count, long totalLength)
        {
            if (mResourceApplyStartEventHandler != null)
            {
                ResourceApplyStartEventArgs resourceApplyStartEventArgs = ResourceApplyStartEventArgs.Create(resourcePackPath, count, totalLength);
                mResourceApplyStartEventHandler(this, resourceApplyStartEventArgs);
                ReferencePool.Release(resourceApplyStartEventArgs);
            }
        }

        private void OnUpdaterResourceApplySuccess(ResourceName resourceName, string applyPath, string resourcePackPath, int length, int compressedLength)
        {
            if (mResourceApplySuccessEventHandler != null)
            {
                ResourceApplySuccessEventArgs resourceApplySuccessEventArgs = ResourceApplySuccessEventArgs.Create(resourceName.FullName, applyPath, resourcePackPath, length, compressedLength);
                mResourceApplySuccessEventHandler(this, resourceApplySuccessEventArgs);
                ReferencePool.Release(resourceApplySuccessEventArgs);
            }
        }

        private void OnUpdaterResourceApplyFailure(ResourceName resourceName, string resourcePackPath, string errorMessage)
        {
            if (mResourceApplyFailureEventHandler != null)
            {
                ResourceApplyFailureEventArgs resourceApplyFailureEventArgs = ResourceApplyFailureEventArgs.Create(resourceName.FullName, resourcePackPath, errorMessage);
                mResourceApplyFailureEventHandler(this, resourceApplyFailureEventArgs);
                ReferencePool.Release(resourceApplyFailureEventArgs);
            }
        }

        private void OnUpdaterResourceApplyComplete(string resourcePackPath, bool result)
        {
            ApplyResourcesCompleteCallback applyResourcesCompleteCallback = mApplyResourcesCompleteCallback;
            mApplyResourcesCompleteCallback = null;
            applyResourcesCompleteCallback(resourcePackPath, result);
        }

        private void OnUpdaterResourceUpdateStart(ResourceName resourceName, string downloadPath, string downloadUri, int currentLength, int compressedLength, int retryCount)
        {
            if (mResourceUpdateStartEventHandler != null)
            {
                ResourceUpdateStartEventArgs resourceUpdateStartEventArgs = ResourceUpdateStartEventArgs.Create(resourceName.FullName, downloadPath, downloadUri, currentLength, compressedLength, retryCount);
                mResourceUpdateStartEventHandler(this, resourceUpdateStartEventArgs);
                ReferencePool.Release(resourceUpdateStartEventArgs);
            }
        }

        private void OnUpdaterResourceUpdateChanged(ResourceName resourceName, string downloadPath, string downloadUri, int currentLength, int compressedLength)
        {
            if (mResourceUpdateChangedEventHandler != null)
            {
                ResourceUpdateChangedEventArgs resourceUpdateChangedEventArgs = ResourceUpdateChangedEventArgs.Create(resourceName.FullName, downloadPath, downloadUri, currentLength, compressedLength);
                mResourceUpdateChangedEventHandler(this, resourceUpdateChangedEventArgs);
                ReferencePool.Release(resourceUpdateChangedEventArgs);
            }
        }

        private void OnUpdaterResourceUpdateSuccess(ResourceName resourceName, string downloadPath, string downloadUri, int length, int compressedLength)
        {
            if (mResourceUpdateSuccessEventHandler != null)
            {
                ResourceUpdateSuccessEventArgs resourceUpdateSuccessEventArgs = ResourceUpdateSuccessEventArgs.Create(resourceName.FullName, downloadPath, downloadUri, length, compressedLength);
                mResourceUpdateSuccessEventHandler(this, resourceUpdateSuccessEventArgs);
                ReferencePool.Release(resourceUpdateSuccessEventArgs);
            }
        }

        private void OnUpdaterResourceUpdateFailure(ResourceName resourceName, string downloadUri, int retryCount, int totalRetryCount, string errorMessage)
        {
            if (mResourceUpdateFailureEventHandler != null)
            {
                ResourceUpdateFailureEventArgs resourceUpdateFailureEventArgs = ResourceUpdateFailureEventArgs.Create(resourceName.FullName, downloadUri, retryCount, totalRetryCount, errorMessage);
                mResourceUpdateFailureEventHandler(this, resourceUpdateFailureEventArgs);
                ReferencePool.Release(resourceUpdateFailureEventArgs);
            }
        }

        private void OnUpdaterResourceUpdateComplete(ResourceGroup resourceGroup, bool result)
        {
            Utility.Path.RemoveEmptyDirectory(mReadWritePath);
            UpdateResourcesCompleteCallback updateResourcesCompleteCallback = mUpdateResourcesCompleteCallback;
            mUpdateResourcesCompleteCallback = null;
            updateResourcesCompleteCallback(resourceGroup, result);
        }

        private void OnUpdaterResourceUpdateAllComplete()
        {
            mResourceUpdater.ResourceApplyStart -= OnUpdaterResourceApplyStart;
            mResourceUpdater.ResourceApplySuccess -= OnUpdaterResourceApplySuccess;
            mResourceUpdater.ResourceApplyFailure -= OnUpdaterResourceApplyFailure;
            mResourceUpdater.ResourceApplyComplete -= OnUpdaterResourceApplyComplete;
            mResourceUpdater.ResourceUpdateStart -= OnUpdaterResourceUpdateStart;
            mResourceUpdater.ResourceUpdateChanged -= OnUpdaterResourceUpdateChanged;
            mResourceUpdater.ResourceUpdateSuccess -= OnUpdaterResourceUpdateSuccess;
            mResourceUpdater.ResourceUpdateFailure -= OnUpdaterResourceUpdateFailure;
            mResourceUpdater.ResourceUpdateComplete -= OnUpdaterResourceUpdateComplete;
            mResourceUpdater.ResourceUpdateAllComplete -= OnUpdaterResourceUpdateAllComplete;
            mResourceUpdater.Shutdown();
            mResourceUpdater = null;

            mReadWriteResourceInfos.Clear();
            mReadWriteResourceInfos = null;

            FreeCachedStream();
            Utility.Path.RemoveEmptyDirectory(mReadWritePath);

            if (mResourceUpdateAllCompleteEventHandler != null)
            {
                ResourceUpdateAllCompleteEventArgs resourceUpdateAllCompleteEventArgs = ResourceUpdateAllCompleteEventArgs.Create();
                mResourceUpdateAllCompleteEventHandler(this, resourceUpdateAllCompleteEventArgs);
                ReferencePool.Release(resourceUpdateAllCompleteEventArgs);
            }
        }
    }
}
