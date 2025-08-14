//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace GameFramework
{
    [StructLayout(LayoutKind.Auto)]
    internal struct TypeNamePair : IEquatable<TypeNamePair>
    {
        public TypeNamePair(Type type)
            : this(type, string.Empty)
        {
        }

        public TypeNamePair(Type type, string name)
        {
            if (type == null)
            {
                throw new GameFrameworkException("Type is invalid.");
            }

            mType = type;
            mName = name ?? string.Empty;
        }



        public override string ToString()
        {
            if (mType == null)
            {
                throw new GameFrameworkException("Type is invalid.");
            }

            string typeName = mType.FullName;
            return string.IsNullOrEmpty(mName) ? typeName : Utility.Text.Format("{0}.{1}", typeName, mName);
        }

        public override int GetHashCode()
        {
            return mType.GetHashCode() ^ mName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TypeNamePair && Equals((TypeNamePair)obj);
        }

        public bool Equals(TypeNamePair value)
        {
            return mType == value.mType && mName == value.mName;
        }

        public static bool operator ==(TypeNamePair a, TypeNamePair b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TypeNamePair a, TypeNamePair b)
        {
            return !(a == b);
        }

        public Type Type
        {
            get
            {
                return mType;
            }
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        private readonly Type mType;
        private readonly string mName;
    }
}