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
        public static class Random
        {
            private static System.Random RandomSystem = new System.Random((int)DateTime.UtcNow.Ticks);

            public static void SetSeed(int seed)
            {
                RandomSystem = new System.Random(seed);
            }

            public static int GetRandom()
            {
                return RandomSystem.Next();
            }

            public static int GetRandom(int maxValue)
            {
                return RandomSystem.Next(maxValue);
            }

            public static int GetRandom(int minValue, int maxValue)
            {
                return RandomSystem.Next(minValue, maxValue);
            }

            public static double GetRandomDouble()
            {
                return RandomSystem.NextDouble();
            }

            public static void GetRandomBytes(byte[] buffer)
            {
                RandomSystem.NextBytes(buffer);
            }
        }
    }
}
