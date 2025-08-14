//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.FileSystem;
using System;

namespace GameFramework.Resource
{
    public interface ILoadResourceAgentHelper
    {
        event EventHandler<LoadResourceAgentHelperUpdateEventArgs> LoadResourceAgentHelperUpdate;

        event EventHandler<LoadResourceAgentHelperReadFileCompleteEventArgs> LoadResourceAgentHelperReadFileComplete;

        event EventHandler<LoadResourceAgentHelperReadBytesCompleteEventArgs> LoadResourceAgentHelperReadBytesComplete;

        event EventHandler<LoadResourceAgentHelperParseBytesCompleteEventArgs> LoadResourceAgentHelperParseBytesComplete;

        event EventHandler<LoadResourceAgentHelperLoadCompleteEventArgs> LoadResourceAgentHelperLoadComplete;

        event EventHandler<LoadResourceAgentHelperErrorEventArgs> LoadResourceAgentHelperError;

        void ReadFile(string fullPath);

        void ReadFile(IFileSystem fileSystem, string name);

        void ReadBytes(string fullPath);

        void ReadBytes(IFileSystem fileSystem, string name);

        void ParseBytes(byte[] bytes);

        void LoadAsset(object resource, string assetName, Type assetType, bool isScene);

        void Reset();
    }
}
