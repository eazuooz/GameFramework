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
        public static partial class Compression
        {
            private static ICompressionHelper s_CompressionHelper = null;

            public static void SetCompressionHelper(ICompressionHelper compressionHelper)
            {
                s_CompressionHelper = compressionHelper;
            }

            public static byte[] Compress(byte[] bytes)
            {
                if (bytes == null)
                {
                    throw new GameFrameworkException("Bytes is invalid.");
                }

                return Compress(bytes, 0, bytes.Length);
            }

            public static bool Compress(byte[] bytes, Stream compressedStream)
            {
                if (bytes == null)
                {
                    throw new GameFrameworkException("Bytes is invalid.");
                }

                return Compress(bytes, 0, bytes.Length, compressedStream);
            }

            public static byte[] Compress(byte[] bytes, int offset, int length)
            {
                using (MemoryStream compressedStream = new MemoryStream())
                {
                    if (Compress(bytes, offset, length, compressedStream))
                    {
                        return compressedStream.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public static bool Compress(byte[] bytes, int offset, int length, Stream compressedStream)
            {
                if (s_CompressionHelper == null)
                {
                    throw new GameFrameworkException("Compressed helper is invalid.");
                }

                if (bytes == null)
                {
                    throw new GameFrameworkException("Bytes is invalid.");
                }

                if (offset < 0 || length < 0 || offset + length > bytes.Length)
                {
                    throw new GameFrameworkException("Offset or length is invalid.");
                }

                if (compressedStream == null)
                {
                    throw new GameFrameworkException("Compressed stream is invalid.");
                }

                try
                {
                    return s_CompressionHelper.Compress(bytes, offset, length, compressedStream);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Text.Format("Can not compress with exception '{0}'.", exception), exception);
                }
            }

            public static byte[] Compress(Stream stream)
            {
                using (MemoryStream compressedStream = new MemoryStream())
                {
                    if (Compress(stream, compressedStream))
                    {
                        return compressedStream.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public static bool Compress(Stream stream, Stream compressedStream)
            {
                if (s_CompressionHelper == null)
                {
                    throw new GameFrameworkException("Compressed helper is invalid.");
                }

                if (stream == null)
                {
                    throw new GameFrameworkException("Stream is invalid.");
                }

                if (compressedStream == null)
                {
                    throw new GameFrameworkException("Compressed stream is invalid.");
                }

                try
                {
                    return s_CompressionHelper.Compress(stream, compressedStream);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Text.Format("Can not compress with exception '{0}'.", exception), exception);
                }
            }

            public static byte[] Decompress(byte[] bytes)
            {
                if (bytes == null)
                {
                    throw new GameFrameworkException("Bytes is invalid.");
                }

                return Decompress(bytes, 0, bytes.Length);
            }

            public static bool Decompress(byte[] bytes, Stream decompressedStream)
            {
                if (bytes == null)
                {
                    throw new GameFrameworkException("Bytes is invalid.");
                }

                return Decompress(bytes, 0, bytes.Length, decompressedStream);
            }

            public static byte[] Decompress(byte[] bytes, int offset, int length)
            {
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    if (Decompress(bytes, offset, length, decompressedStream))
                    {
                        return decompressedStream.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public static bool Decompress(byte[] bytes, int offset, int length, Stream decompressedStream)
            {
                if (s_CompressionHelper == null)
                {
                    throw new GameFrameworkException("Compressed helper is invalid.");
                }

                if (bytes == null)
                {
                    throw new GameFrameworkException("Bytes is invalid.");
                }

                if (offset < 0 || length < 0 || offset + length > bytes.Length)
                {
                    throw new GameFrameworkException("Offset or length is invalid.");
                }

                if (decompressedStream == null)
                {
                    throw new GameFrameworkException("Decompressed stream is invalid.");
                }

                try
                {
                    return s_CompressionHelper.Decompress(bytes, offset, length, decompressedStream);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Text.Format("Can not decompress with exception '{0}'.", exception), exception);
                }
            }

            public static byte[] Decompress(Stream stream)
            {
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    if (Decompress(stream, decompressedStream))
                    {
                        return decompressedStream.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public static bool Decompress(Stream stream, Stream decompressedStream)
            {
                if (s_CompressionHelper == null)
                {
                    throw new GameFrameworkException("Compressed helper is invalid.");
                }

                if (stream == null)
                {
                    throw new GameFrameworkException("Stream is invalid.");
                }

                if (decompressedStream == null)
                {
                    throw new GameFrameworkException("Decompressed stream is invalid.");
                }

                try
                {
                    return s_CompressionHelper.Decompress(stream, decompressedStream);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Text.Format("Can not decompress with exception '{0}'.", exception), exception);
                }
            }
        }
    }
}
