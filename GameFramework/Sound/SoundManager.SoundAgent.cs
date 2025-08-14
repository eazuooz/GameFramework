//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.Sound
{
    internal sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        private sealed class SoundAgent : ISoundAgent
        {
            private readonly SoundGroup mSoundGroup;
            private readonly ISoundHelper mSoundHelper;
            private readonly ISoundAgentHelper mSoundAgentHelper;
            private int mSerialId;
            private object mSoundAsset;
            private DateTime mSetSoundAssetTime;
            private bool mMuteInSoundGroup;
            private float mVolumeInSoundGroup;

            public SoundAgent(SoundGroup soundGroup, ISoundHelper soundHelper, ISoundAgentHelper soundAgentHelper)
            {
                if (soundGroup == null)
                {
                    throw new GameFrameworkException("Sound group is invalid.");
                }

                if (soundHelper == null)
                {
                    throw new GameFrameworkException("Sound helper is invalid.");
                }

                if (soundAgentHelper == null)
                {
                    throw new GameFrameworkException("Sound agent helper is invalid.");
                }

                mSoundGroup = soundGroup;
                mSoundHelper = soundHelper;
                mSoundAgentHelper = soundAgentHelper;
                mSoundAgentHelper.ResetSoundAgent += OnResetSoundAgent;
                mSerialId = 0;
                mSoundAsset = null;
                Reset();
            }

            public ISoundGroup SoundGroup
            {
                get
                {
                    return mSoundGroup;
                }
            }

            public int SerialId
            {
                get
                {
                    return mSerialId;
                }
                set
                {
                    mSerialId = value;
                }
            }

            public bool IsPlaying
            {
                get
                {
                    return mSoundAgentHelper.IsPlaying;
                }
            }

            public float Length
            {
                get
                {
                    return mSoundAgentHelper.Length;
                }
            }

            public float Time
            {
                get
                {
                    return mSoundAgentHelper.Time;
                }
                set
                {
                    mSoundAgentHelper.Time = value;
                }
            }

            public bool Mute
            {
                get
                {
                    return mSoundAgentHelper.Mute;
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
                    RefreshMute();
                }
            }

            public bool Loop
            {
                get
                {
                    return mSoundAgentHelper.Loop;
                }
                set
                {
                    mSoundAgentHelper.Loop = value;
                }
            }

            public int Priority
            {
                get
                {
                    return mSoundAgentHelper.Priority;
                }
                set
                {
                    mSoundAgentHelper.Priority = value;
                }
            }

            public float Volume
            {
                get
                {
                    return mSoundAgentHelper.Volume;
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
                    RefreshVolume();
                }
            }

            public float Pitch
            {
                get
                {
                    return mSoundAgentHelper.Pitch;
                }
                set
                {
                    mSoundAgentHelper.Pitch = value;
                }
            }

            public float PanStereo
            {
                get
                {
                    return mSoundAgentHelper.PanStereo;
                }
                set
                {
                    mSoundAgentHelper.PanStereo = value;
                }
            }

            public float SpatialBlend
            {
                get
                {
                    return mSoundAgentHelper.SpatialBlend;
                }
                set
                {
                    mSoundAgentHelper.SpatialBlend = value;
                }
            }

            public float MaxDistance
            {
                get
                {
                    return mSoundAgentHelper.MaxDistance;
                }
                set
                {
                    mSoundAgentHelper.MaxDistance = value;
                }
            }

            public float DopplerLevel
            {
                get
                {
                    return mSoundAgentHelper.DopplerLevel;
                }
                set
                {
                    mSoundAgentHelper.DopplerLevel = value;
                }
            }

            public ISoundAgentHelper Helper
            {
                get
                {
                    return mSoundAgentHelper;
                }
            }

            internal DateTime SetSoundAssetTime
            {
                get
                {
                    return mSetSoundAssetTime;
                }
            }

            public void Play()
            {
                mSoundAgentHelper.Play(Constant.DefaultFadeInSeconds);
            }

            public void Play(float fadeInSeconds)
            {
                mSoundAgentHelper.Play(fadeInSeconds);
            }

            public void Stop()
            {
                mSoundAgentHelper.Stop(Constant.DefaultFadeOutSeconds);
            }

            public void Stop(float fadeOutSeconds)
            {
                mSoundAgentHelper.Stop(fadeOutSeconds);
            }

            public void Pause()
            {
                mSoundAgentHelper.Pause(Constant.DefaultFadeOutSeconds);
            }

            public void Pause(float fadeOutSeconds)
            {
                mSoundAgentHelper.Pause(fadeOutSeconds);
            }

            public void Resume()
            {
                mSoundAgentHelper.Resume(Constant.DefaultFadeInSeconds);
            }

            public void Resume(float fadeInSeconds)
            {
                mSoundAgentHelper.Resume(fadeInSeconds);
            }

            public void Reset()
            {
                if (mSoundAsset != null)
                {
                    mSoundHelper.ReleaseSoundAsset(mSoundAsset);
                    mSoundAsset = null;
                }

                mSetSoundAssetTime = DateTime.MinValue;
                Time = Constant.DefaultTime;
                MuteInSoundGroup = Constant.DefaultMute;
                Loop = Constant.DefaultLoop;
                Priority = Constant.DefaultPriority;
                VolumeInSoundGroup = Constant.DefaultVolume;
                Pitch = Constant.DefaultPitch;
                PanStereo = Constant.DefaultPanStereo;
                SpatialBlend = Constant.DefaultSpatialBlend;
                MaxDistance = Constant.DefaultMaxDistance;
                DopplerLevel = Constant.DefaultDopplerLevel;
                mSoundAgentHelper.Reset();
            }

            internal bool SetSoundAsset(object soundAsset)
            {
                Reset();
                mSoundAsset = soundAsset;
                mSetSoundAssetTime = DateTime.UtcNow;
                return mSoundAgentHelper.SetSoundAsset(soundAsset);
            }

            internal void RefreshMute()
            {
                mSoundAgentHelper.Mute = mSoundGroup.Mute || mMuteInSoundGroup;
            }

            internal void RefreshVolume()
            {
                mSoundAgentHelper.Volume = mSoundGroup.Volume * mVolumeInSoundGroup;
            }

            private void OnResetSoundAgent(object sender, ResetSoundAgentEventArgs e)
            {
                Reset();
            }
        }
    }
}
