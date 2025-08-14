//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.IO;

namespace GameFramework.FileSystem
{
    public sealed class CommonFileSystemStream : FileSystemStream, IDisposable
    {
        private readonly FileStream mFileStream;

        public CommonFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            switch (access)
            {
                case FileSystemAccess.Read:
                    mFileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    break;

                case FileSystemAccess.Write:
                    mFileStream = new FileStream(fullPath, createNew ? FileMode.Create : FileMode.Open, FileAccess.Write, FileShare.Read);
                    break;

                case FileSystemAccess.ReadWrite:
                    mFileStream = new FileStream(fullPath, createNew ? FileMode.Create : FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    break;

                default:
                    throw new GameFrameworkException("Access is invalid.");
            }
        }

        protected internal override long Position
        {
            get
            {
                return mFileStream.Position;
            }
            set
            {
                mFileStream.Position = value;
            }
        }

        protected internal override long Length
        {
            get
            {
                return mFileStream.Length;
            }
        }

        protected internal override void SetLength(long length)
        {
            mFileStream.SetLength(length);
        }

        protected internal override void Seek(long offset, SeekOrigin origin)
        {
            mFileStream.Seek(offset, origin);
        }

        protected internal override int ReadByte()
        {
            return mFileStream.ReadByte();
        }

        protected internal override int Read(byte[] buffer, int startIndex, int length)
        {
            return mFileStream.Read(buffer, startIndex, length);
        }

        protected internal override void WriteByte(byte value)
        {
            mFileStream.WriteByte(value);
        }

        protected internal override void Write(byte[] buffer, int startIndex, int length)
        {
            mFileStream.Write(buffer, startIndex, length);
        }

        protected internal override void Flush()
        {
            mFileStream.Flush();
        }

        protected internal override void Close()
        {
            mFileStream.Close();
        }

        public void Dispose()
        {
            mFileStream.Dispose();
        }
    }
}
