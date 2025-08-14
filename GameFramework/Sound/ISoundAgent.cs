//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Sound
{
    public interface ISoundAgent
    {
        ISoundGroup SoundGroup
        {
            get;
        }

        int SerialId
        {
            get;
        }

        bool IsPlaying
        {
            get;
        }

        float Length
        {
            get;
        }

        float Time
        {
            get;
            set;
        }

        bool Mute
        {
            get;
        }

        bool MuteInSoundGroup
        {
            get;
            set;
        }

        bool Loop
        {
            get;
            set;
        }

        int Priority
        {
            get;
            set;
        }

        float Volume
        {
            get;
        }

        float VolumeInSoundGroup
        {
            get;
            set;
        }

        float Pitch
        {
            get;
            set;
        }

        float PanStereo
        {
            get;
            set;
        }

        float SpatialBlend
        {
            get;
            set;
        }

        float MaxDistance
        {
            get;
            set;
        }

        float DopplerLevel
        {
            get;
            set;
        }

        ISoundAgentHelper Helper
        {
            get;
        }

        void Play();

        void Play(float fadeInSeconds);

        void Stop();

        void Stop(float fadeOutSeconds);

        void Pause();

        void Pause(float fadeOutSeconds);

        void Resume();

        void Resume(float fadeInSeconds);

        void Reset();
    }
}
