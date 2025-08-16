//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    public static partial class Version
    {
        public static void SetVersionHelper(IVersionHelper versionHelper)
        {
            VersionHelper = versionHelper;
        }

        public static string GameFrameworkVersion
        {
            get
            {
                return GameFrameworkVersionString;
            }
        }

        public static string GameVersion
        {
            get
            {
                if (VersionHelper == null)
                {
                    return string.Empty;
                }

                return VersionHelper.GameVersion;
            }
        }

        public static int InternalGameVersion
        {
            get
            {
                if (VersionHelper == null)
                {
                    return 0;
                }

                return VersionHelper.InternalGameVersion;
            }
        }

        private const string GameFrameworkVersionString = "2021.05.31";

        private static IVersionHelper VersionHelper = null;
    }
}