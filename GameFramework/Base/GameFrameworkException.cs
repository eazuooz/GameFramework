//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace GameFramework
{
    [Serializable]
    public class GameFrameworkException : Exception
    {
        public GameFrameworkException()
            : base()
        {
        }

        public GameFrameworkException(string message)
            : base(message)
        {
        }

        public GameFrameworkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GameFrameworkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
