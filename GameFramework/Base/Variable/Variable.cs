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
    public abstract class Variable : IReference
    {
        public Variable()
        {
        }

        public abstract object GetValue();

        public abstract void SetValue(object value);

        public abstract void Clear();

        public abstract Type Type
        {
            get;
        }
    }
}