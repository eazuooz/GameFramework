//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameFramework.FileSystem
{
    [StructLayout(LayoutKind.Auto)]
    public struct FileInfo
    {
        private readonly string mName;
        private readonly long mOffset;
        private readonly int mLength;

        public FileInfo(string name, long offset, int length)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0L)
            {
                throw new GameFrameworkException("Offset is invalid.");
            }

            if (length < 0)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            mName = name;
            mOffset = offset;
            mLength = length;
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(mName) && mOffset >= 0L && mLength >= 0;
            }
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public long Offset
        {
            get
            {
                return mOffset;
            }
        }

        public int Length
        {
            get
            {
                return mLength;
            }
        }
    }
}
