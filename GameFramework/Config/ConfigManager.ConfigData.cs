//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameFramework.Config
{
    internal sealed partial class ConfigManager : GameFrameworkModule, IConfigManager
    {
        [StructLayout(LayoutKind.Auto)]
        private struct ConfigData
        {
            private readonly bool mBoolValue;
            private readonly int mIntValue;
            private readonly float mFloatValue;
            private readonly string mStringValue;

            public ConfigData(bool boolValue, int intValue, float floatValue, string stringValue)
            {
                mBoolValue = boolValue;
                mIntValue = intValue;
                mFloatValue = floatValue;
                mStringValue = stringValue;
            }

            public bool BoolValue
            {
                get
                {
                    return mBoolValue;
                }
            }

            public int IntValue
            {
                get
                {
                    return mIntValue;
                }
            }

            public float FloatValue
            {
                get
                {
                    return mFloatValue;
                }
            }

            public string StringValue
            {
                get
                {
                    return mStringValue;
                }
            }
        }
    }
}
