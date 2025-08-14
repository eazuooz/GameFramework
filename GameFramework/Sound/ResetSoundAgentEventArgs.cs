//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Sound
{
    public sealed class ResetSoundAgentEventArgs : GameFrameworkEventArgs
    {
        public ResetSoundAgentEventArgs()
        {
        }

        public static ResetSoundAgentEventArgs Create()
        {
            ResetSoundAgentEventArgs resetSoundAgentEventArgs = ReferencePool.Acquire<ResetSoundAgentEventArgs>();
            return resetSoundAgentEventArgs;
        }

        public override void Clear()
        {
        }
    }
}
