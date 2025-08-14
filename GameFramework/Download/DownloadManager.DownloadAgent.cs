//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.IO;

namespace GameFramework.Download
{
    internal sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        private sealed class DownloadAgent : ITaskAgent<DownloadTask>, IDisposable
        {
            private readonly IDownloadAgentHelper mHelper;
            private DownloadTask mTask;
            private FileStream mFileStream;
            private int mWaitFlushSize;
            private float mWaitTime;
            private long mStartLength;
            private long mDownloadedLength;
            private long mSavedLength;
            private bool mDisposed;

            public GameFrameworkAction<DownloadAgent> DownloadAgentStart;
            public GameFrameworkAction<DownloadAgent, int> DownloadAgentUpdate;
            public GameFrameworkAction<DownloadAgent, long> DownloadAgentSuccess;
            public GameFrameworkAction<DownloadAgent, string> DownloadAgentFailure;

            public DownloadAgent(IDownloadAgentHelper downloadAgentHelper)
            {
                if (downloadAgentHelper == null)
                {
                    throw new GameFrameworkException("Download agent helper is invalid.");
                }

                mHelper = downloadAgentHelper;
                mTask = null;
                mFileStream = null;
                mWaitFlushSize = 0;
                mWaitTime = 0f;
                mStartLength = 0L;
                mDownloadedLength = 0L;
                mSavedLength = 0L;
                mDisposed = false;

                DownloadAgentStart = null;
                DownloadAgentUpdate = null;
                DownloadAgentSuccess = null;
                DownloadAgentFailure = null;
            }

            public DownloadTask Task
            {
                get
                {
                    return mTask;
                }
            }

            public float WaitTime
            {
                get
                {
                    return mWaitTime;
                }
            }

            public long StartLength
            {
                get
                {
                    return mStartLength;
                }
            }

            public long DownloadedLength
            {
                get
                {
                    return mDownloadedLength;
                }
            }

            public long CurrentLength
            {
                get
                {
                    return mStartLength + mDownloadedLength;
                }
            }

            public long SavedLength
            {
                get
                {
                    return mSavedLength;
                }
            }

            public void Initialize()
            {
                mHelper.DownloadAgentHelperUpdateBytes += OnDownloadAgentHelperUpdateBytes;
                mHelper.DownloadAgentHelperUpdateLength += OnDownloadAgentHelperUpdateLength;
                mHelper.DownloadAgentHelperComplete += OnDownloadAgentHelperComplete;
                mHelper.DownloadAgentHelperError += OnDownloadAgentHelperError;
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (mTask.Status == DownloadTaskStatus.Doing)
                {
                    mWaitTime += realElapseSeconds;
                    if (mWaitTime >= mTask.Timeout)
                    {
                        DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(false, "Timeout");
                        OnDownloadAgentHelperError(this, downloadAgentHelperErrorEventArgs);
                        ReferencePool.Release(downloadAgentHelperErrorEventArgs);
                    }
                }
            }

            public void Shutdown()
            {
                Dispose();

                mHelper.DownloadAgentHelperUpdateBytes -= OnDownloadAgentHelperUpdateBytes;
                mHelper.DownloadAgentHelperUpdateLength -= OnDownloadAgentHelperUpdateLength;
                mHelper.DownloadAgentHelperComplete -= OnDownloadAgentHelperComplete;
                mHelper.DownloadAgentHelperError -= OnDownloadAgentHelperError;
            }

            public StartTaskStatus Start(DownloadTask task)
            {
                if (task == null)
                {
                    throw new GameFrameworkException("Task is invalid.");
                }

                mTask = task;

                mTask.Status = DownloadTaskStatus.Doing;
                string downloadFile = Utility.Text.Format("{0}.download", mTask.DownloadPath);

                try
                {
                    if (File.Exists(downloadFile))
                    {
                        mFileStream = File.OpenWrite(downloadFile);
                        mFileStream.Seek(0L, SeekOrigin.End);
                        mStartLength = mSavedLength = mFileStream.Length;
                        mDownloadedLength = 0L;
                    }
                    else
                    {
                        string directory = Path.GetDirectoryName(mTask.DownloadPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        mFileStream = new FileStream(downloadFile, FileMode.Create, FileAccess.Write);
                        mStartLength = mSavedLength = mDownloadedLength = 0L;
                    }

                    if (DownloadAgentStart != null)
                    {
                        DownloadAgentStart(this);
                    }

                    if (mStartLength > 0L)
                    {
                        mHelper.Download(mTask.DownloadUri, mStartLength, mTask.UserData);
                    }
                    else
                    {
                        mHelper.Download(mTask.DownloadUri, mTask.UserData);
                    }

                    return StartTaskStatus.CanResume;
                }
                catch (Exception exception)
                {
                    DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(false, exception.ToString());
                    OnDownloadAgentHelperError(this, downloadAgentHelperErrorEventArgs);
                    ReferencePool.Release(downloadAgentHelperErrorEventArgs);
                    return StartTaskStatus.UnknownError;
                }
            }

            public void Reset()
            {
                mHelper.Reset();

                if (mFileStream != null)
                {
                    mFileStream.Close();
                    mFileStream = null;
                }

                mTask = null;
                mWaitFlushSize = 0;
                mWaitTime = 0f;
                mStartLength = 0L;
                mDownloadedLength = 0L;
                mSavedLength = 0L;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (mDisposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (mFileStream != null)
                    {
                        mFileStream.Dispose();
                        mFileStream = null;
                    }
                }

                mDisposed = true;
            }

            private void OnDownloadAgentHelperUpdateBytes(object sender, DownloadAgentHelperUpdateBytesEventArgs e)
            {
                mWaitTime = 0f;
                try
                {
                    mFileStream.Write(e.GetBytes(), e.Offset, e.Length);
                    mWaitFlushSize += e.Length;
                    mSavedLength += e.Length;

                    if (mWaitFlushSize >= mTask.FlushSize)
                    {
                        mFileStream.Flush();
                        mWaitFlushSize = 0;
                    }
                }
                catch (Exception exception)
                {
                    DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(false, exception.ToString());
                    OnDownloadAgentHelperError(this, downloadAgentHelperErrorEventArgs);
                    ReferencePool.Release(downloadAgentHelperErrorEventArgs);
                }
            }

            private void OnDownloadAgentHelperUpdateLength(object sender, DownloadAgentHelperUpdateLengthEventArgs e)
            {
                mWaitTime = 0f;
                mDownloadedLength += e.DeltaLength;
                if (DownloadAgentUpdate != null)
                {
                    DownloadAgentUpdate(this, e.DeltaLength);
                }
            }

            private void OnDownloadAgentHelperComplete(object sender, DownloadAgentHelperCompleteEventArgs e)
            {
                mWaitTime = 0f;
                mDownloadedLength = e.Length;
                if (mSavedLength != CurrentLength)
                {
                    throw new GameFrameworkException("Internal download error.");
                }

                mHelper.Reset();
                mFileStream.Close();
                mFileStream = null;

                if (File.Exists(mTask.DownloadPath))
                {
                    File.Delete(mTask.DownloadPath);
                }

                File.Move(Utility.Text.Format("{0}.download", mTask.DownloadPath), mTask.DownloadPath);

                mTask.Status = DownloadTaskStatus.Done;

                if (DownloadAgentSuccess != null)
                {
                    DownloadAgentSuccess(this, e.Length);
                }

                mTask.Done = true;
            }

            private void OnDownloadAgentHelperError(object sender, DownloadAgentHelperErrorEventArgs e)
            {
                mHelper.Reset();
                if (mFileStream != null)
                {
                    mFileStream.Close();
                    mFileStream = null;
                }

                if (e.DeleteDownloading)
                {
                    File.Delete(Utility.Text.Format("{0}.download", mTask.DownloadPath));
                }

                mTask.Status = DownloadTaskStatus.Error;

                if (DownloadAgentFailure != null)
                {
                    DownloadAgentFailure(this, e.ErrorMessage);
                }

                mTask.Done = true;
            }
        }
    }
}
