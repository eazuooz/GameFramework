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
    public interface IDownloadManager
    {
        bool Paused
        {
            get;
            set;
        }

        int TotalAgentCount
        {
            get;
        }

        int FreeAgentCount
        {
            get;
        }

        int WorkingAgentCount
        {
            get;
        }

        int WaitingTaskCount
        {
            get;
        }

        int FlushSize
        {
            get;
            set;
        }

        float Timeout
        {
            get;
            set;
        }

        float CurrentSpeed
        {
            get;
        }

        event EventHandler<DownloadStartEventArgs> DownloadStart;

        event EventHandler<DownloadUpdateEventArgs> DownloadUpdate;

        event EventHandler<DownloadSuccessEventArgs> DownloadSuccess;

        event EventHandler<DownloadFailureEventArgs> DownloadFailure;

        void AddDownloadAgentHelper(IDownloadAgentHelper downloadAgentHelper);

        TaskInfo GetDownloadInfo(int serialId);

        TaskInfo[] GetDownloadInfos(string tag);

        void GetDownloadInfos(string tag, List<TaskInfo> results);

        TaskInfo[] GetAllDownloadInfos();

        void GetAllDownloadInfos(List<TaskInfo> results);

        int AddDownload(string downloadPath, string downloadUri);

        int AddDownload(string downloadPath, string downloadUri, string tag);

        int AddDownload(string downloadPath, string downloadUri, int priority);

        int AddDownload(string downloadPath, string downloadUri, object userData);

        int AddDownload(string downloadPath, string downloadUri, string tag, int priority);

        int AddDownload(string downloadPath, string downloadUri, string tag, object userData);

        int AddDownload(string downloadPath, string downloadUri, int priority, object userData);

        int AddDownload(string downloadPath, string downloadUri, string tag, int priority, object userData);

        bool RemoveDownload(int serialId);

        int RemoveDownloads(string tag);

        int RemoveAllDownloads();
    }
}
