//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    public static partial class GameFrameworkLog
    {
        public interface ILogHelper
        {
            void Log(GameFrameworkLogLevel level, object message);
        }
    }
}
