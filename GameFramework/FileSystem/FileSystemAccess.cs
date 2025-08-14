//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.FileSystem
{
    [Flags]
    public enum FileSystemAccess : byte
    {
        Unspecified = 0,

        Read = 1,

        Write = 2,

        ReadWrite = 3
    }
}
