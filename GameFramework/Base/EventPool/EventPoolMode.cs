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
    [Flags]
    internal enum EventPoolMode : byte
    {
        Default = 0,

        AllowNoHandler = 1,

        AllowMultiHandler = 2,

        AllowDuplicateHandler = 4
    }
}