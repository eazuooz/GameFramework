//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.Sound
{
    internal sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        private sealed class SoundGroup : ISoundGroup
        {
            private readonly string mName;
            private readonly ISoundGroupHelper mSoundGroupHelper;
            private readonly List<SoundAgent> mSoundAgents;
            private bool mAvoidBeingReplacedBySamePriority;
            private bool mMute;
            private float mVolume;

            public SoundGroup(string name, ISoundGroupHelper soundGroupHelper)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Sound group name is invalid.");
                }

                if (soundGroupHelper == null)
                {
                    throw new GameFrameworkException("Sound group helper is invalid.");
                }

                mName = name;
                mSoundGroupHelper = soundGroupHelper;
                mSoundAgents = new List<SoundAgent>();
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public int SoundAgentCount
            {
                get
                {
                    return mSoundAgents.Count;
                }
            }

            public bool AvoidBeingReplacedBySamePriority
            {
                get
                {
                    return mAvoidBeingReplacedBySamePriority;
                }
                set
                {
                    mAvoidBeingReplacedBySamePriority = value;
                }
            }

            public bool Mute
            {
                get
                {
                    return mMute;
                }
                set
                {
                    mMute = value;
                    foreach (SoundAgent soundAgent in mSoundAgents)
                    {
                        soundAgent.RefreshMute();
                    }
                }
            }

            public float Volume
            {
                get
                {
                    return mVolume;
                }
                set
                {
                    mVolume = value;
                    foreach (SoundAgent soundAgent in mSoundAgents)
                    {
                        soundAgent.RefreshVolume();
                    }
                }
            }

            public ISoundGroupHelper Helper
            {
                get
                {
                    return mSoundGroupHelper;
                }
            }

            public void AddSoundAgentHelper(ISoundHelper soundHelper, ISoundAgentHelper soundAgentHelper)
            {
                mSoundAgents.Add(new SoundAgent(this, soundHelper, soundAgentHelper));
            }

            public ISoundAgent PlaySound(int serialId, object soundAsset, PlaySoundParams playSoundParams, out PlaySoundErrorCode? errorCode)
            {
                errorCode = null;
                SoundAgent candidateAgent = null;
                foreach (SoundAgent soundAgent in mSoundAgents)
                {
                    if (!soundAgent.IsPlaying)
                    {
                        candidateAgent = soundAgent;
                        break;
                    }

                    if (soundAgent.Priority < playSoundParams.Priority)
                    {
                        if (candidateAgent == null || soundAgent.Priority < candidateAgent.Priority)
                        {
                            candidateAgent = soundAgent;
                        }
                    }
                    else if (!mAvoidBeingReplacedBySamePriority && soundAgent.Priority == playSoundParams.Priority)
                    {
                        if (candidateAgent == null || soundAgent.SetSoundAssetTime < candidateAgent.SetSoundAssetTime)
                        {
                            candidateAgent = soundAgent;
                        }
                    }
                }

                if (candidateAgent == null)
                {
                    errorCode = PlaySoundErrorCode.IgnoredDueToLowPriority;
                    return null;
                }

                if (!candidateAgent.SetSoundAsset(soundAsset))
                {
                    errorCode = PlaySoundErrorCode.SetSoundAssetFailure;
                    return null;
                }

                candidateAgent.SerialId = serialId;
                candidateAgent.Time = playSoundParams.Time;
                candidateAgent.MuteInSoundGroup = playSoundParams.MuteInSoundGroup;
                candidateAgent.Loop = playSoundParams.Loop;
                candidateAgent.Priority = playSoundParams.Priority;
                candidateAgent.VolumeInSoundGroup = playSoundParams.VolumeInSoundGroup;
                candidateAgent.Pitch = playSoundParams.Pitch;
                candidateAgent.PanStereo = playSoundParams.PanStereo;
                candidateAgent.SpatialBlend = playSoundParams.SpatialBlend;
                candidateAgent.MaxDistance = playSoundParams.MaxDistance;
                candidateAgent.DopplerLevel = playSoundParams.DopplerLevel;
                candidateAgent.Play(playSoundParams.FadeInSeconds);
                return candidateAgent;
            }

            public bool StopSound(int serialId, float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in mSoundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Stop(fadeOutSeconds);
                    return true;
                }

                return false;
            }

            public bool PauseSound(int serialId, float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in mSoundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Pause(fadeOutSeconds);
                    return true;
                }

                return false;
            }

            public bool ResumeSound(int serialId, float fadeInSeconds)
            {
                foreach (SoundAgent soundAgent in mSoundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Resume(fadeInSeconds);
                    return true;
                }

                return false;
            }

            public void StopAllLoadedSounds()
            {
                foreach (SoundAgent soundAgent in mSoundAgents)
                {
                    if (soundAgent.IsPlaying)
                    {
                        soundAgent.Stop();
                    }
                }
            }

            public void StopAllLoadedSounds(float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in mSoundAgents)
                {
                    if (soundAgent.IsPlaying)
                    {
                        soundAgent.Stop(fadeOutSeconds);
                    }
                }
            }
        }
    }
}
