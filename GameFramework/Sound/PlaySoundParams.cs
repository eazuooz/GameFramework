//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Sound
{
    public sealed class PlaySoundParams : IReference
    {
        private bool mReferenced;
        private float mTime;
        private bool mMuteInSoundGroup;
        private bool mLoop;
        private int mPriority;
        private float mVolumeInSoundGroup;
        private float mFadeInSeconds;
        private float mPitch;
        private float mPanStereo;
        private float mSpatialBlend;
        private float mMaxDistance;
        private float mDopplerLevel;

        public PlaySoundParams()
        {
            mReferenced = false;
            mTime = Constant.DefaultTime;
            mMuteInSoundGroup = Constant.DefaultMute;
            mLoop = Constant.DefaultLoop;
            mPriority = Constant.DefaultPriority;
            mVolumeInSoundGroup = Constant.DefaultVolume;
            mFadeInSeconds = Constant.DefaultFadeInSeconds;
            mPitch = Constant.DefaultPitch;
            mPanStereo = Constant.DefaultPanStereo;
            mSpatialBlend = Constant.DefaultSpatialBlend;
            mMaxDistance = Constant.DefaultMaxDistance;
            mDopplerLevel = Constant.DefaultDopplerLevel;
        }

        public float Time
        {
            get
            {
                return mTime;
            }
            set
            {
                mTime = value;
            }
        }

        public bool MuteInSoundGroup
        {
            get
            {
                return mMuteInSoundGroup;
            }
            set
            {
                mMuteInSoundGroup = value;
            }
        }

        public bool Loop
        {
            get
            {
                return mLoop;
            }
            set
            {
                mLoop = value;
            }
        }

        public int Priority
        {
            get
            {
                return mPriority;
            }
            set
            {
                mPriority = value;
            }
        }

        public float VolumeInSoundGroup
        {
            get
            {
                return mVolumeInSoundGroup;
            }
            set
            {
                mVolumeInSoundGroup = value;
            }
        }

        public float FadeInSeconds
        {
            get
            {
                return mFadeInSeconds;
            }
            set
            {
                mFadeInSeconds = value;
            }
        }

        public float Pitch
        {
            get
            {
                return mPitch;
            }
            set
            {
                mPitch = value;
            }
        }

        public float PanStereo
        {
            get
            {
                return mPanStereo;
            }
            set
            {
                mPanStereo = value;
            }
        }

        public float SpatialBlend
        {
            get
            {
                return mSpatialBlend;
            }
            set
            {
                mSpatialBlend = value;
            }
        }

        public float MaxDistance
        {
            get
            {
                return mMaxDistance;
            }
            set
            {
                mMaxDistance = value;
            }
        }

        public float DopplerLevel
        {
            get
            {
                return mDopplerLevel;
            }
            set
            {
                mDopplerLevel = value;
            }
        }

        internal bool Referenced
        {
            get
            {
                return mReferenced;
            }
        }

        public static PlaySoundParams Create()
        {
            PlaySoundParams playSoundParams = ReferencePool.Acquire<PlaySoundParams>();
            playSoundParams.mReferenced = true;
            return playSoundParams;
        }

        public void Clear()
        {
            mTime = Constant.DefaultTime;
            mMuteInSoundGroup = Constant.DefaultMute;
            mLoop = Constant.DefaultLoop;
            mPriority = Constant.DefaultPriority;
            mVolumeInSoundGroup = Constant.DefaultVolume;
            mFadeInSeconds = Constant.DefaultFadeInSeconds;
            mPitch = Constant.DefaultPitch;
            mPanStereo = Constant.DefaultPanStereo;
            mSpatialBlend = Constant.DefaultSpatialBlend;
            mMaxDistance = Constant.DefaultMaxDistance;
            mDopplerLevel = Constant.DefaultDopplerLevel;
        }
    }
}
