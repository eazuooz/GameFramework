//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Sound
{
    public sealed class PlaySoundUpdateEventArgs : GameFrameworkEventArgs
    {
        public PlaySoundUpdateEventArgs()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            Progress = 0f;
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

        public float Progress
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static PlaySoundUpdateEventArgs Create(int serialId, string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams, float progress, object userData)
        {
            PlaySoundUpdateEventArgs playSoundUpdateEventArgs = ReferencePool.Acquire<PlaySoundUpdateEventArgs>();
            playSoundUpdateEventArgs.SerialId = serialId;
            playSoundUpdateEventArgs.SoundAssetName = soundAssetName;
            playSoundUpdateEventArgs.SoundGroupName = soundGroupName;
            playSoundUpdateEventArgs.PlaySoundParams = playSoundParams;
            playSoundUpdateEventArgs.Progress = progress;
            playSoundUpdateEventArgs.UserData = userData;
            return playSoundUpdateEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            SoundAssetName = null;
            SoundGroupName = null;
            PlaySoundParams = null;
            Progress = 0f;
            UserData = null;
        }
    }
}
