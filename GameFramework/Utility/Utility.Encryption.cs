//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework
{
    public static partial class Utility
    {
        public static class Encryption
        {
            internal const int QuickEncryptLength = 220;

            public static byte[] GetQuickXorBytes(byte[] bytes, byte[] code)
            {
                return GetXorBytes(bytes, 0, QuickEncryptLength, code);
            }

            public static void GetQuickSelfXorBytes(byte[] bytes, byte[] code)
            {
                GetSelfXorBytes(bytes, 0, QuickEncryptLength, code);
            }

            public static byte[] GetXorBytes(byte[] bytes, byte[] code)
            {
                if (bytes == null)
                {
                    return null;
                }

                return GetXorBytes(bytes, 0, bytes.Length, code);
            }

            public static void GetSelfXorBytes(byte[] bytes, byte[] code)
            {
                if (bytes == null)
                {
                    return;
                }

                GetSelfXorBytes(bytes, 0, bytes.Length, code);
            }

            public static byte[] GetXorBytes(byte[] bytes, int startIndex, int length, byte[] code)
            {
                if (bytes == null)
                {
                    return null;
                }

                int bytesLength = bytes.Length;
                byte[] results = new byte[bytesLength];
                Array.Copy(bytes, 0, results, 0, bytesLength);
                GetSelfXorBytes(results, startIndex, length, code);
                return results;
            }

            public static void GetSelfXorBytes(byte[] bytes, int startIndex, int length, byte[] code)
            {
                if (bytes == null)
                {
                    return;
                }

                if (code == null)
                {
                    throw new GameFrameworkException("Code is invalid.");
                }

                int codeLength = code.Length;
                if (codeLength <= 0)
                {
                    throw new GameFrameworkException("Code length is invalid.");
                }

                if (startIndex < 0 || length < 0 || startIndex + length > bytes.Length)
                {
                    throw new GameFrameworkException("Start index or length is invalid.");
                }

                int codeIndex = startIndex % codeLength;
                for (int i = startIndex; i < length; i++)
                {
                    bytes[i] ^= code[codeIndex++];
                    codeIndex %= codeLength;
                }
            }
        }
    }
}
