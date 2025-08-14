//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Sound
{
    internal sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        private sealed class PlaySoundInfo : IReference
        {
            private int mSerialId;
            private SoundGroup mSoundGroup;
            private PlaySoundParams mPlaySoundParams;
            private object mUserData;

            public PlaySoundInfo()
            {
                mSerialId = 0;
                mSoundGroup = null;
                mPlaySoundParams = null;
                mUserData = null;
            }

            public int SerialId
            {
                get
                {
                    return mSerialId;
                }
            }

            public SoundGroup SoundGroup
            {
                get
                {
                    return mSoundGroup;
                }
            }

            public PlaySoundParams PlaySoundParams
            {
                get
                {
                    return mPlaySoundParams;
                }
            }

            public object UserData
            {
                get
                {
                    return mUserData;
                }
            }

            public static PlaySoundInfo Create(int serialId, SoundGroup soundGroup, PlaySoundParams playSoundParams, object userData)
            {
                PlaySoundInfo playSoundInfo = ReferencePool.Acquire<PlaySoundInfo>();
                playSoundInfo.mSerialId = serialId;
                playSoundInfo.mSoundGroup = soundGroup;
                playSoundInfo.mPlaySoundParams = playSoundParams;
                playSoundInfo.mUserData = userData;
                return playSoundInfo;
            }

            public void Clear()
            {
                mSerialId = 0;
                mSoundGroup = null;
                mPlaySoundParams = null;
                mUserData = null;
            }
        }
    }
}
