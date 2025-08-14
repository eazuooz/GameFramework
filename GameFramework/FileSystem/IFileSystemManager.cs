//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.FileSystem
{
    public interface IFileSystemManager
    {
        int Count
        {
            get;
        }

        void SetFileSystemHelper(IFileSystemHelper fileSystemHelper);

        bool HasFileSystem(string fullPath);

        IFileSystem GetFileSystem(string fullPath);

        IFileSystem CreateFileSystem(string fullPath, FileSystemAccess access, int maxFileCount, int maxBlockCount);

        IFileSystem LoadFileSystem(string fullPath, FileSystemAccess access);

        void DestroyFileSystem(IFileSystem fileSystem, bool deletePhysicalFile);

        IFileSystem[] GetAllFileSystems();

        void GetAllFileSystems(List<IFileSystem> results);
    }
}
