//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Download;
using System;
using System.IO;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed class VersionListProcessor
        {
            private readonly ResourceManager mResourceManager;
            private IDownloadManager mDownloadManager;
            private int mVersionListLength;
            private int mVersionListHashCode;
            private int mVersionListCompressedLength;
            private int mVersionListCompressedHashCode;

            public GameFrameworkAction<string, string> VersionListUpdateSuccess;
            public GameFrameworkAction<string, string> VersionListUpdateFailure;

            public VersionListProcessor(ResourceManager resourceManager)
            {
                mResourceManager = resourceManager;
                mDownloadManager = null;
                mVersionListLength = 0;
                mVersionListHashCode = 0;
                mVersionListCompressedLength = 0;
                mVersionListCompressedHashCode = 0;

                VersionListUpdateSuccess = null;
                VersionListUpdateFailure = null;
            }

            public void Shutdown()
            {
                if (mDownloadManager != null)
                {
                    mDownloadManager.DownloadSuccess -= OnDownloadSuccess;
                    mDownloadManager.DownloadFailure -= OnDownloadFailure;
                }
            }

            public void SetDownloadManager(IDownloadManager downloadManager)
            {
                if (downloadManager == null)
                {
                    throw new GameFrameworkException("Download manager is invalid.");
                }

                mDownloadManager = downloadManager;
                mDownloadManager.DownloadSuccess += OnDownloadSuccess;
                mDownloadManager.DownloadFailure += OnDownloadFailure;
            }

            public CheckVersionListResult CheckVersionList(int latestInternalResourceVersion)
            {
                if (string.IsNullOrEmpty(mResourceManager.mReadWritePath))
                {
                    throw new GameFrameworkException("Read-write path is invalid.");
                }

                string versionListFileName = Utility.Path.GetRegularPath(Path.Combine(mResourceManager.mReadWritePath, RemoteVersionListFileName));
                if (!File.Exists(versionListFileName))
                {
                    return CheckVersionListResult.NeedUpdate;
                }

                int internalResourceVersion = 0;
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(versionListFileName, FileMode.Open, FileAccess.Read);
                    object internalResourceVersionObject = null;
                    if (!mResourceManager.mUpdatableVersionListSerializer.TryGetValue(fileStream, "InternalResourceVersion", out internalResourceVersionObject))
                    {
                        return CheckVersionListResult.NeedUpdate;
                    }

                    internalResourceVersion = (int)internalResourceVersionObject;
                }
                catch
                {
                    return CheckVersionListResult.NeedUpdate;
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }

                if (internalResourceVersion != latestInternalResourceVersion)
                {
                    return CheckVersionListResult.NeedUpdate;
                }

                return CheckVersionListResult.Updated;
            }

            public void UpdateVersionList(int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
            {
                if (mDownloadManager == null)
                {
                    throw new GameFrameworkException("You must set download manager first.");
                }

                mVersionListLength = versionListLength;
                mVersionListHashCode = versionListHashCode;
                mVersionListCompressedLength = versionListCompressedLength;
                mVersionListCompressedHashCode = versionListCompressedHashCode;
                string localVersionListFilePath = Utility.Path.GetRegularPath(Path.Combine(mResourceManager.mReadWritePath, RemoteVersionListFileName));
                int dotPosition = RemoteVersionListFileName.LastIndexOf('.');
                string latestVersionListFullNameWithCrc32 = Utility.Text.Format("{0}.{2:x8}.{1}", RemoteVersionListFileName.Substring(0, dotPosition), RemoteVersionListFileName.Substring(dotPosition + 1), mVersionListHashCode);
                mDownloadManager.AddDownload(localVersionListFilePath, Utility.Path.GetRemotePath(Path.Combine(mResourceManager.mUpdatePrefixUri, latestVersionListFullNameWithCrc32)), this);
            }

            private void OnDownloadSuccess(object sender, DownloadSuccessEventArgs e)
            {
                VersionListProcessor versionListProcessor = e.UserData as VersionListProcessor;
                if (versionListProcessor == null || versionListProcessor != this)
                {
                    return;
                }

                try
                {
                    using (FileStream fileStream = new FileStream(e.DownloadPath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        int length = (int)fileStream.Length;
                        if (length != mVersionListCompressedLength)
                        {
                            fileStream.Close();
                            string errorMessage = Utility.Text.Format("Latest version list compressed length error, need '{0}', downloaded '{1}'.", mVersionListCompressedLength, length);
                            DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                            OnDownloadFailure(this, downloadFailureEventArgs);
                            ReferencePool.Release(downloadFailureEventArgs);
                            return;
                        }

                        fileStream.Position = 0L;
                        int hashCode = Utility.Verifier.GetCrc32(fileStream);
                        if (hashCode != mVersionListCompressedHashCode)
                        {
                            fileStream.Close();
                            string errorMessage = Utility.Text.Format("Latest version list compressed hash code error, need '{0}', downloaded '{1}'.", mVersionListCompressedHashCode, hashCode);
                            DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                            OnDownloadFailure(this, downloadFailureEventArgs);
                            ReferencePool.Release(downloadFailureEventArgs);
                            return;
                        }

                        fileStream.Position = 0L;
                        mResourceManager.PrepareCachedStream();
                        if (!Utility.Compression.Decompress(fileStream, mResourceManager.mCachedStream))
                        {
                            fileStream.Close();
                            string errorMessage = Utility.Text.Format("Unable to decompress latest version list '{0}'.", e.DownloadPath);
                            DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                            OnDownloadFailure(this, downloadFailureEventArgs);
                            ReferencePool.Release(downloadFailureEventArgs);
                            return;
                        }

                        int uncompressedLength = (int)mResourceManager.mCachedStream.Length;
                        if (uncompressedLength != mVersionListLength)
                        {
                            fileStream.Close();
                            string errorMessage = Utility.Text.Format("Latest version list length error, need '{0}', downloaded '{1}'.", mVersionListLength, uncompressedLength);
                            DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                            OnDownloadFailure(this, downloadFailureEventArgs);
                            ReferencePool.Release(downloadFailureEventArgs);
                            return;
                        }

                        fileStream.Position = 0L;
                        fileStream.SetLength(0L);
                        fileStream.Write(mResourceManager.mCachedStream.GetBuffer(), 0, uncompressedLength);
                    }

                    if (VersionListUpdateSuccess != null)
                    {
                        VersionListUpdateSuccess(e.DownloadPath, e.DownloadUri);
                    }
                }
                catch (Exception exception)
                {
                    string errorMessage = Utility.Text.Format("Update latest version list '{0}' with error message '{1}'.", e.DownloadPath, exception);
                    DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(e.SerialId, e.DownloadPath, e.DownloadUri, errorMessage, e.UserData);
                    OnDownloadFailure(this, downloadFailureEventArgs);
                    ReferencePool.Release(downloadFailureEventArgs);
                }
            }

            private void OnDownloadFailure(object sender, DownloadFailureEventArgs e)
            {
                VersionListProcessor versionListProcessor = e.UserData as VersionListProcessor;
                if (versionListProcessor == null || versionListProcessor != this)
                {
                    return;
                }

                if (File.Exists(e.DownloadPath))
                {
                    File.Delete(e.DownloadPath);
                }

                if (VersionListUpdateFailure != null)
                {
                    VersionListUpdateFailure(e.DownloadUri, e.ErrorMessage);
                }
            }
        }
    }
}
