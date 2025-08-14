//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;
using System.IO;

namespace GameFramework.FileSystem
{
    public interface IFileSystem
    {
        string FullPath
        {
            get;
        }

        FileSystemAccess Access
        {
            get;
        }

        int FileCount
        {
            get;
        }

        int MaxFileCount
        {
            get;
        }

        FileInfo GetFileInfo(string name);

        FileInfo[] GetAllFileInfos();

        void GetAllFileInfos(List<FileInfo> results);

        bool HasFile(string name);

        byte[] ReadFile(string name);

        int ReadFile(string name, byte[] buffer);

        int ReadFile(string name, byte[] buffer, int startIndex);

        int ReadFile(string name, byte[] buffer, int startIndex, int length);

        int ReadFile(string name, Stream stream);

        byte[] ReadFileSegment(string name, int length);

        byte[] ReadFileSegment(string name, int offset, int length);

        int ReadFileSegment(string name, byte[] buffer);

        int ReadFileSegment(string name, byte[] buffer, int length);

        int ReadFileSegment(string name, byte[] buffer, int startIndex, int length);

        int ReadFileSegment(string name, int offset, byte[] buffer);

        int ReadFileSegment(string name, int offset, byte[] buffer, int length);

        int ReadFileSegment(string name, int offset, byte[] buffer, int startIndex, int length);

        int ReadFileSegment(string name, Stream stream, int length);

        int ReadFileSegment(string name, int offset, Stream stream, int length);

        bool WriteFile(string name, byte[] buffer);

        bool WriteFile(string name, byte[] buffer, int startIndex);

        bool WriteFile(string name, byte[] buffer, int startIndex, int length);

        bool WriteFile(string name, Stream stream);

        bool WriteFile(string name, string filePath);

        bool SaveAsFile(string name, string filePath);

        bool RenameFile(string oldName, string newName);

        bool DeleteFile(string name);
    }
}
