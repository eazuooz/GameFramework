//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.IO;

namespace GameFramework
{
    public static partial class Utility
    {
        public static partial class Verifier
        {
            private const int CachedBytesLength = 0x1000;
            private static readonly byte[] s_CachedBytes = new byte[CachedBytesLength];
            private static readonly Crc32 s_Algorithm = new Crc32();

            public static int GetCrc32(byte[] bytes)
            {
                if (bytes == null)
                {
                    throw new GameFrameworkException("Bytes is invalid.");
                }

                return GetCrc32(bytes, 0, bytes.Length);
            }

            public static int GetCrc32(byte[] bytes, int offset, int length)
            {
                if (bytes == null)
                {
                    throw new GameFrameworkException("Bytes is invalid.");
                }

                if (offset < 0 || length < 0 || offset + length > bytes.Length)
                {
                    throw new GameFrameworkException("Offset or length is invalid.");
                }

                s_Algorithm.HashCore(bytes, offset, length);
                int result = (int)s_Algorithm.HashFinal();
                s_Algorithm.Initialize();
                return result;
            }

            public static int GetCrc32(Stream stream)
            {
                if (stream == null)
                {
                    throw new GameFrameworkException("Stream is invalid.");
                }

                while (true)
                {
                    int bytesRead = stream.Read(s_CachedBytes, 0, CachedBytesLength);
                    if (bytesRead > 0)
                    {
                        s_Algorithm.HashCore(s_CachedBytes, 0, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }

                int result = (int)s_Algorithm.HashFinal();
                s_Algorithm.Initialize();
                Array.Clear(s_CachedBytes, 0, CachedBytesLength);
                return result;
            }

            public static byte[] GetCrc32Bytes(int crc32)
            {
                return new byte[] { (byte)((crc32 >> 24) & 0xff), (byte)((crc32 >> 16) & 0xff), (byte)((crc32 >> 8) & 0xff), (byte)(crc32 & 0xff) };
            }

            public static void GetCrc32Bytes(int crc32, byte[] bytes)
            {
                GetCrc32Bytes(crc32, bytes, 0);
            }

            public static void GetCrc32Bytes(int crc32, byte[] bytes, int offset)
            {
                if (bytes == null)
                {
                    throw new GameFrameworkException("Result is invalid.");
                }

                if (offset < 0 || offset + 4 > bytes.Length)
                {
                    throw new GameFrameworkException("Offset or length is invalid.");
                }

                bytes[offset] = (byte)((crc32 >> 24) & 0xff);
                bytes[offset + 1] = (byte)((crc32 >> 16) & 0xff);
                bytes[offset + 2] = (byte)((crc32 >> 8) & 0xff);
                bytes[offset + 3] = (byte)(crc32 & 0xff);
            }

            internal static int GetCrc32(Stream stream, byte[] code, int length)
            {
                if (stream == null)
                {
                    throw new GameFrameworkException("Stream is invalid.");
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

                int bytesLength = (int)stream.Length;
                if (length < 0 || length > bytesLength)
                {
                    length = bytesLength;
                }

                int codeIndex = 0;
                while (true)
                {
                    int bytesRead = stream.Read(s_CachedBytes, 0, CachedBytesLength);
                    if (bytesRead > 0)
                    {
                        if (length > 0)
                        {
                            for (int i = 0; i < bytesRead && i < length; i++)
                            {
                                s_CachedBytes[i] ^= code[codeIndex++];
                                codeIndex %= codeLength;
                            }

                            length -= bytesRead;
                        }

                        s_Algorithm.HashCore(s_CachedBytes, 0, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }

                int result = (int)s_Algorithm.HashFinal();
                s_Algorithm.Initialize();
                Array.Clear(s_CachedBytes, 0, CachedBytesLength);
                return result;
            }
        }
    }
}
