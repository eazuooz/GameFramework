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
    public abstract class Variable<T> : Variable
    {
        private T mValue;

        public Variable()
        {
            mValue = default(T);
        }

        public override Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        public T Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }

        public override object GetValue()
        {
            return mValue;
        }

        public override void SetValue(object value)
        {
            mValue = (T)value;
        }

        public override void Clear()
        {
            mValue = default(T);
        }

        public override string ToString()
        {
            return (mValue != null) ? mValue.ToString() : "<Null>";
        }
    }
}