//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework.Download
{
    internal sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        private const int OneMegaBytes = 1024 * 1024;

        private readonly TaskPool<DownloadTask> mTaskPool;
        private readonly DownloadCounter mDownloadCounter;
        private int mFlushSize;
        private float mTimeout;
        private EventHandler<DownloadStartEventArgs> mDownloadStartEventHandler;
        private EventHandler<DownloadUpdateEventArgs> mDownloadUpdateEventHandler;
        private EventHandler<DownloadSuccessEventArgs> mDownloadSuccessEventHandler;
        private EventHandler<DownloadFailureEventArgs> mDownloadFailureEventHandler;

        public DownloadManager()
        {
            mTaskPool = new TaskPool<DownloadTask>();
            mDownloadCounter = new DownloadCounter(1f, 10f);
            mFlushSize = OneMegaBytes;
            mTimeout = 30f;
            mDownloadStartEventHandler = null;
            mDownloadUpdateEventHandler = null;
            mDownloadSuccessEventHandler = null;
            mDownloadFailureEventHandler = null;
        }

        internal override int Priority
        {
            get
            {
                return 5;
            }
        }

        public bool Paused
        {
            get
            {
                return mTaskPool.Paused;
            }
            set
            {
                mTaskPool.Paused = value;
            }
        }

        public int TotalAgentCount
        {
            get
            {
                return mTaskPool.TotalAgentCount;
            }
        }

        public int FreeAgentCount
        {
            get
            {
                return mTaskPool.FreeAgentCount;
            }
        }

        public int WorkingAgentCount
        {
            get
            {
                return mTaskPool.WorkingAgentCount;
            }
        }

        public int WaitingTaskCount
        {
            get
            {
                return mTaskPool.WaitingTaskCount;
            }
        }

        public int FlushSize
        {
            get
            {
                return mFlushSize;
            }
            set
            {
                mFlushSize = value;
            }
        }

        public float Timeout
        {
            get
            {
                return mTimeout;
            }
            set
            {
                mTimeout = value;
            }
        }

        public float CurrentSpeed
        {
            get
            {
                return mDownloadCounter.CurrentSpeed;
            }
        }

        public event EventHandler<DownloadStartEventArgs> DownloadStart
        {
            add
            {
                mDownloadStartEventHandler += value;
            }
            remove
            {
                mDownloadStartEventHandler -= value;
            }
        }

        public event EventHandler<DownloadUpdateEventArgs> DownloadUpdate
        {
            add
            {
                mDownloadUpdateEventHandler += value;
            }
            remove
            {
                mDownloadUpdateEventHandler -= value;
            }
        }

        public event EventHandler<DownloadSuccessEventArgs> DownloadSuccess
        {
            add
            {
                mDownloadSuccessEventHandler += value;
            }
            remove
            {
                mDownloadSuccessEventHandler -= value;
            }
        }

        public event EventHandler<DownloadFailureEventArgs> DownloadFailure
        {
            add
            {
                mDownloadFailureEventHandler += value;
            }
            remove
            {
                mDownloadFailureEventHandler -= value;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            mTaskPool.Update(elapseSeconds, realElapseSeconds);
            mDownloadCounter.Update(elapseSeconds, realElapseSeconds);
        }

        internal override void Shutdown()
        {
            mTaskPool.Shutdown();
            mDownloadCounter.Shutdown();
        }

        public void AddDownloadAgentHelper(IDownloadAgentHelper downloadAgentHelper)
        {
            DownloadAgent agent = new DownloadAgent(downloadAgentHelper);
            agent.DownloadAgentStart += OnDownloadAgentStart;
            agent.DownloadAgentUpdate += OnDownloadAgentUpdate;
            agent.DownloadAgentSuccess += OnDownloadAgentSuccess;
            agent.DownloadAgentFailure += OnDownloadAgentFailure;

            mTaskPool.AddAgent(agent);
        }

        public TaskInfo GetDownloadInfo(int serialId)
        {
            return mTaskPool.GetTaskInfo(serialId);
        }

        public TaskInfo[] GetDownloadInfos(string tag)
        {
            return mTaskPool.GetTaskInfos(tag);
        }

        public void GetDownloadInfos(string tag, List<TaskInfo> results)
        {
            mTaskPool.GetTaskInfos(tag, results);
        }

        public TaskInfo[] GetAllDownloadInfos()
        {
            return mTaskPool.GetAllTaskInfos();
        }

        public void GetAllDownloadInfos(List<TaskInfo> results)
        {
            mTaskPool.GetAllTaskInfos(results);
        }

        public int AddDownload(string downloadPath, string downloadUri)
        {
            return AddDownload(downloadPath, downloadUri, null, Constant.DefaultPriority, null);
        }

        public int AddDownload(string downloadPath, string downloadUri, string tag)
        {
            return AddDownload(downloadPath, downloadUri, tag, Constant.DefaultPriority, null);
        }

        public int AddDownload(string downloadPath, string downloadUri, int priority)
        {
            return AddDownload(downloadPath, downloadUri, null, priority, null);
        }

        public int AddDownload(string downloadPath, string downloadUri, object userData)
        {
            return AddDownload(downloadPath, downloadUri, null, Constant.DefaultPriority, userData);
        }

        public int AddDownload(string downloadPath, string downloadUri, string tag, int priority)
        {
            return AddDownload(downloadPath, downloadUri, tag, priority, null);
        }

        public int AddDownload(string downloadPath, string downloadUri, string tag, object userData)
        {
            return AddDownload(downloadPath, downloadUri, tag, Constant.DefaultPriority, userData);
        }

        public int AddDownload(string downloadPath, string downloadUri, int priority, object userData)
        {
            return AddDownload(downloadPath, downloadUri, null, priority, userData);
        }

        public int AddDownload(string downloadPath, string downloadUri, string tag, int priority, object userData)
        {
            if (string.IsNullOrEmpty(downloadPath))
            {
                throw new GameFrameworkException("Download path is invalid.");
            }

            if (string.IsNullOrEmpty(downloadUri))
            {
                throw new GameFrameworkException("Download uri is invalid.");
            }

            if (TotalAgentCount <= 0)
            {
                throw new GameFrameworkException("You must add download agent first.");
            }

            DownloadTask downloadTask = DownloadTask.Create(downloadPath, downloadUri, tag, priority, mFlushSize, mTimeout, userData);
            mTaskPool.AddTask(downloadTask);
            return downloadTask.SerialId;
        }

        public bool RemoveDownload(int serialId)
        {
            return mTaskPool.RemoveTask(serialId);
        }

        public int RemoveDownloads(string tag)
        {
            return mTaskPool.RemoveTasks(tag);
        }

        public int RemoveAllDownloads()
        {
            return mTaskPool.RemoveAllTasks();
        }

        private void OnDownloadAgentStart(DownloadAgent sender)
        {
            if (mDownloadStartEventHandler != null)
            {
                DownloadStartEventArgs downloadStartEventArgs = DownloadStartEventArgs.Create(sender.Task.SerialId, sender.Task.DownloadPath, sender.Task.DownloadUri, sender.CurrentLength, sender.Task.UserData);
                mDownloadStartEventHandler(this, downloadStartEventArgs);
                ReferencePool.Release(downloadStartEventArgs);
            }
        }

        private void OnDownloadAgentUpdate(DownloadAgent sender, int deltaLength)
        {
            mDownloadCounter.RecordDeltaLength(deltaLength);
            if (mDownloadUpdateEventHandler != null)
            {
                DownloadUpdateEventArgs downloadUpdateEventArgs = DownloadUpdateEventArgs.Create(sender.Task.SerialId, sender.Task.DownloadPath, sender.Task.DownloadUri, sender.CurrentLength, sender.Task.UserData);
                mDownloadUpdateEventHandler(this, downloadUpdateEventArgs);
                ReferencePool.Release(downloadUpdateEventArgs);
            }
        }

        private void OnDownloadAgentSuccess(DownloadAgent sender, long length)
        {
            if (mDownloadSuccessEventHandler != null)
            {
                DownloadSuccessEventArgs downloadSuccessEventArgs = DownloadSuccessEventArgs.Create(sender.Task.SerialId, sender.Task.DownloadPath, sender.Task.DownloadUri, sender.CurrentLength, sender.Task.UserData);
                mDownloadSuccessEventHandler(this, downloadSuccessEventArgs);
                ReferencePool.Release(downloadSuccessEventArgs);
            }
        }

        private void OnDownloadAgentFailure(DownloadAgent sender, string errorMessage)
        {
            if (mDownloadFailureEventHandler != null)
            {
                DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(sender.Task.SerialId, sender.Task.DownloadPath, sender.Task.DownloadUri, errorMessage, sender.Task.UserData);
                mDownloadFailureEventHandler(this, downloadFailureEventArgs);
                ReferencePool.Release(downloadFailureEventArgs);
            }
        }
    }
}
