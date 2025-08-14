//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Sound
{
    public sealed class PlaySoundFailureEventArgs : GameFrameworkEventArgs
    {
        public PlaySoundFailureEventArgs()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            ErrorCode = PlaySoundErrorCode.Unknown;
            ErrorMessage = null;
            UserData = null;
        }

        public int SerialId
        {
            get;
            private set;
        }

        public string SoundAssetName
        {
            get;
            private set;
        }

        public string SoundGroupName
        {
            get;
            private set;
        }

        public PlaySoundParams PlaySoundParams
        {
            get;
            private set;
        }

        public PlaySoundErrorCode ErrorCode
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static PlaySoundFailureEventArgs Create(int serialId, string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams, PlaySoundErrorCode errorCode, string errorMessage, object userData)
        {
            PlaySoundFailureEventArgs playSoundFailureEventArgs = ReferencePool.Acquire<PlaySoundFailureEventArgs>();
            playSoundFailureEventArgs.SerialId = serialId;
            playSoundFailureEventArgs.SoundAssetName = soundAssetName;
            playSoundFailureEventArgs.SoundGroupName = soundGroupName;
            playSoundFailureEventArgs.PlaySoundParams = playSoundParams;
            playSoundFailureEventArgs.ErrorCode = errorCode;
            playSoundFailureEventArgs.ErrorMessage = errorMessage;
            playSoundFailureEventArgs.UserData = userData;
            return playSoundFailureEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            ErrorCode = PlaySoundErrorCode.Unknown;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
