//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.Download
{
    public interface IDownloadAgentHelper
    {
        event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes;

        event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength;

        event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete;

        event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError;

        void Download(string downloadUri, object userData);

        void Download(string downloadUri, long fromPosition, object userData);

        void Download(string downloadUri, long fromPosition, long toPosition, object userData);

        void Reset();
    }
}
