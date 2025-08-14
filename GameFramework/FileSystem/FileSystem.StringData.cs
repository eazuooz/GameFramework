//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace GameFramework.FileSystem
{
    internal sealed partial class FileSystem : IFileSystem
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct StringData
        {
            private static readonly byte[] s_CachedBytes = new byte[byte.MaxValue + 1];

            private readonly byte mLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = byte.MaxValue)]
            private readonly byte[] mBytes;

            public StringData(byte length, byte[] bytes)
            {
                mLength = length;
                mBytes = bytes;
            }

            public string GetString(byte[] encryptBytes)
            {
                if (mLength <= 0)
                {
                    return null;
                }

                Array.Copy(mBytes, 0, s_CachedBytes, 0, mLength);
                Utility.Encryption.GetSelfXorBytes(s_CachedBytes, 0, mLength, encryptBytes);
                return Utility.Converter.GetString(s_CachedBytes, 0, mLength);
            }

            public StringData SetString(string value, byte[] encryptBytes)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return Clear();
                }

                int length = Utility.Converter.GetBytes(value, s_CachedBytes);
                if (length > byte.MaxValue)
                {
                    throw new GameFrameworkException(Utility.Text.Format("String '{0}' is too long.", value));
                }

                Utility.Encryption.GetSelfXorBytes(s_CachedBytes, encryptBytes);
                Array.Copy(s_CachedBytes, 0, mBytes, 0, length);
                return new StringData((byte)length, mBytes);
            }

            public StringData Clear()
            {
                return new StringData(0, mBytes);
            }
        }
    }
}
