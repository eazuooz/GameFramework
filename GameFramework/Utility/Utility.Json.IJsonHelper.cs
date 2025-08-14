//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework
{
    public static partial class Utility
    {
        public static partial class Json
        {
            public interface IJsonHelper
            {
                string ToJson(object obj);

                T ToObject<T>(string json);

                object ToObject(Type objectType, string json);
            }
        }
    }
}
