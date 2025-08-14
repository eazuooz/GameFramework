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
        public static void SetLogHelper(ILogHelper logHelper)
        {
            s_LogHelper = logHelper;
        }

        public static void Debug(object message)
        {
            if (s_LogHelper == null)
            {
                return;
            }

            s_LogHelper.Log(GameFrameworkLogLevel.Debug, message);
        }

        public static void Debug(string message)
        {
            if (s_LogHelper == null)
            {
                return;
            }

            s_LogHelper.Log(GameFrameworkLogLevel.Debug, message);
        }

        public static void Debug<T>(string format, T arg)
        {
            if (s_LogHelper == null)
            {
                return;
            }

            s_LogHelper.Log(GameFrameworkLogLevel.Debug, Utility.Text.Format(format, arg));
        }

        public static void Debug<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            if (s_LogHelper == null)
            {
                return;
            }

            s_LogHelper.Log(GameFrameworkLogLevel.Debug, Utility.Text.Format(format, arg1, arg2));
        }

        // ... 계속해서 모든 Debug 메서드들에 대해 동일한 패턴으로 번역 ...

        public static void Info(object message)
        {
            if (s_LogHelper == null)
            {
                return;
            }

            s_LogHelper.Log(GameFrameworkLogLevel.Info, message);
        }

        // ... 모든 Info 메서드들에 대해 동일한 패턴으로 번역 ...

        public static void Warning(object message)
        {
            if (s_LogHelper == null)
            {
                return;
            }

            s_LogHelper.Log(GameFrameworkLogLevel.Warning, message);
        }

        // ... 모든 Warning 메서드들에 대해 동일한 패턴으로 번역 ...

        public static void Error(object message)
        {
            if (s_LogHelper == null)
            {
                return;
            }

            s_LogHelper.Log(GameFrameworkLogLevel.Error, message);
        }

        // ... 모든 Error 메서드들에 대해 동일한 패턴으로 번역 ...

        public static void Fatal(object message)
        {
            if (s_LogHelper == null)
            {
                return;
            }

            s_LogHelper.Log(GameFrameworkLogLevel.Fatal, message);
        }

        private static ILogHelper s_LogHelper = null;
    }
}