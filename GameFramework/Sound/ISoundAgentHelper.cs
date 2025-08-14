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
    public interface ISoundAgentHelper
    {
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

        event EventHandler<ResetSoundAgentEventArgs> ResetSoundAgent;

        void Play(float fadeInSeconds);

        void Stop(float fadeOutSeconds);

        void Pause(float fadeOutSeconds);

        void Resume(float fadeInSeconds);

        void Reset();

        bool SetSoundAsset(object soundAsset);
    }
}
