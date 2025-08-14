//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public enum LoadResourceStatus : byte
    {
        Success = 0,

        NotExist,

        NotReady,

        DependencyError,

        TypeError,

        AssetError
    }
}
