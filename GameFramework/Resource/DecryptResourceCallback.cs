//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public delegate void DecryptResourceCallback(byte[] bytes, int startIndex, int count, string name, string variant, string extension, bool storageInReadOnly, string fileSystem, byte loadType, int length, int hashCode);
}
