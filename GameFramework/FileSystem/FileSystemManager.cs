//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace GameFramework.FileSystem
{
    internal sealed class FileSystemManager : GameFrameworkModule, IFileSystemManager
    {
        private readonly Dictionary<string, FileSystem> mFileSystems;

        private IFileSystemHelper mFileSystemHelper;

        public FileSystemManager()
        {
            mFileSystems = new Dictionary<string, FileSystem>(StringComparer.Ordinal);
            mFileSystemHelper = null;
        }

        internal override int Priority
        {
            get
            {
                return 4;
            }
        }

        public int Count
        {
            get
            {
                return mFileSystems.Count;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            while (mFileSystems.Count > 0)
            {
                foreach (KeyValuePair<string, FileSystem> fileSystem in mFileSystems)
                {
                    DestroyFileSystem(fileSystem.Value, false);
                    break;
                }
            }
        }

        public void SetFileSystemHelper(IFileSystemHelper fileSystemHelper)
        {
            if (fileSystemHelper == null)
            {
                throw new GameFrameworkException("File system helper is invalid.");
            }

            mFileSystemHelper = fileSystemHelper;
        }

        public bool HasFileSystem(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            return mFileSystems.ContainsKey(Utility.Path.GetRegularPath(fullPath));
        }

        public IFileSystem GetFileSystem(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            FileSystem fileSystem = null;
            if (mFileSystems.TryGetValue(Utility.Path.GetRegularPath(fullPath), out fileSystem))
            {
                return fileSystem;
            }

            return null;
        }

        public IFileSystem CreateFileSystem(string fullPath, FileSystemAccess access, int maxFileCount, int maxBlockCount)
        {
            if (mFileSystemHelper == null)
            {
                throw new GameFrameworkException("File system helper is invalid.");
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            if (access == FileSystemAccess.Unspecified)
            {
                throw new GameFrameworkException("Access is invalid.");
            }

            if (access == FileSystemAccess.Read)
            {
                throw new GameFrameworkException("Access read is invalid.");
            }

            fullPath = Utility.Path.GetRegularPath(fullPath);
            if (mFileSystems.ContainsKey(fullPath))
            {
                throw new GameFrameworkException(Utility.Text.Format("File system '{0}' is already exist.", fullPath));
            }

            FileSystemStream fileSystemStream = mFileSystemHelper.CreateFileSystemStream(fullPath, access, true);
            if (fileSystemStream == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Create file system stream for '{0}' failure.", fullPath));
            }

            FileSystem fileSystem = FileSystem.Create(fullPath, access, fileSystemStream, maxFileCount, maxBlockCount);
            if (fileSystem == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Create file system '{0}' failure.", fullPath));
            }

            mFileSystems.Add(fullPath, fileSystem);
            return fileSystem;
        }

        public IFileSystem LoadFileSystem(string fullPath, FileSystemAccess access)
        {
            if (mFileSystemHelper == null)
            {
                throw new GameFrameworkException("File system helper is invalid.");
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            if (access == FileSystemAccess.Unspecified)
            {
                throw new GameFrameworkException("Access is invalid.");
            }

            fullPath = Utility.Path.GetRegularPath(fullPath);
            if (mFileSystems.ContainsKey(fullPath))
            {
                throw new GameFrameworkException(Utility.Text.Format("File system '{0}' is already exist.", fullPath));
            }

            FileSystemStream fileSystemStream = mFileSystemHelper.CreateFileSystemStream(fullPath, access, false);
            if (fileSystemStream == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Create file system stream for '{0}' failure.", fullPath));
            }

            FileSystem fileSystem = FileSystem.Load(fullPath, access, fileSystemStream);
            if (fileSystem == null)
            {
                fileSystemStream.Close();
                throw new GameFrameworkException(Utility.Text.Format("Load file system '{0}' failure.", fullPath));
            }

            mFileSystems.Add(fullPath, fileSystem);
            return fileSystem;
        }

        public void DestroyFileSystem(IFileSystem fileSystem, bool deletePhysicalFile)
        {
            if (fileSystem == null)
            {
                throw new GameFrameworkException("File system is invalid.");
            }

            string fullPath = fileSystem.FullPath;
            ((FileSystem)fileSystem).Shutdown();
            mFileSystems.Remove(fullPath);

            if (deletePhysicalFile && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public IFileSystem[] GetAllFileSystems()
        {
            int index = 0;
            IFileSystem[] results = new IFileSystem[mFileSystems.Count];
            foreach (KeyValuePair<string, FileSystem> fileSystem in mFileSystems)
            {
                results[index++] = fileSystem.Value;
            }

            return results;
        }

        public void GetAllFileSystems(List<IFileSystem> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, FileSystem> fileSystem in mFileSystems)
            {
                results.Add(fileSystem.Value);
            }
        }
    }
}
