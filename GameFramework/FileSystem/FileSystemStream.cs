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
    public abstract class FileSystemStream
    {
        protected const int CachedBytesLength = 0x1000;

        protected static readonly byte[] s_CachedBytes = new byte[CachedBytesLength];

        protected internal abstract long Position
        {
            get;
            set;
        }

        protected internal abstract long Length
        {
            get;
        }

        protected internal abstract void SetLength(long length);

        protected internal abstract void Seek(long offset, SeekOrigin origin);

        protected internal abstract int ReadByte();

        protected internal abstract int Read(byte[] buffer, int startIndex, int length);

        protected internal int Read(Stream stream, int length)
        {
            int bytesRead = 0;
            int bytesLeft = length;
            while ((bytesRead = Read(s_CachedBytes, 0, bytesLeft < CachedBytesLength ? bytesLeft : CachedBytesLength)) > 0)
            {
                bytesLeft -= bytesRead;
                stream.Write(s_CachedBytes, 0, bytesRead);
            }

            Array.Clear(s_CachedBytes, 0, CachedBytesLength);
            return length - bytesLeft;
        }

        protected internal abstract void WriteByte(byte value);

        protected internal abstract void Write(byte[] buffer, int startIndex, int length);

        protected internal void Write(Stream stream, int length)
        {
            int bytesRead = 0;
            int bytesLeft = length;
            while ((bytesRead = stream.Read(s_CachedBytes, 0, bytesLeft < CachedBytesLength ? bytesLeft : CachedBytesLength)) > 0)
            {
                bytesLeft -= bytesRead;
                Write(s_CachedBytes, 0, bytesRead);
            }

            Array.Clear(s_CachedBytes, 0, CachedBytesLength);
        }

        protected internal abstract void Flush();

        protected internal abstract void Close();
    }
}
