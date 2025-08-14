//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Sound
{
    public enum PlaySoundErrorCode : byte
    {
        Unknown = 0,

        SoundGroupNotExist,

        SoundGroupHasNoAgent,

        LoadAssetFailure,

        IgnoredDueToLowPriority,

        SetSoundAssetFailure
    }
}
