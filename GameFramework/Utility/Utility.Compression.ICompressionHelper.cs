//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.IO;

namespace GameFramework
{
    public static partial class Utility
    {
        public static partial class Compression
        {
            public interface ICompressionHelper
            {
                bool Compress(byte[] bytes, int offset, int length, Stream compressedStream);

                bool Compress(Stream stream, Stream compressedStream);

                bool Decompress(byte[] bytes, int offset, int length, Stream decompressedStream);

                bool Decompress(Stream stream, Stream decompressedStream);
            }
        }
    }
}
