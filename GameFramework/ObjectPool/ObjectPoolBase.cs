//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.ObjectPool
{
    public abstract class ObjectPoolBase
    {
        private readonly string mName;

        public ObjectPoolBase()
            : this(null)
        {
        }

        public ObjectPoolBase(string name)
        {
            mName = name ?? string.Empty;
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public string FullName
        {
            get
            {
                return new TypeNamePair(ObjectType, mName).ToString();
            }
        }

        public abstract Type ObjectType
        {
            get;
        }

        public abstract int Count
        {
            get;
        }

        public abstract int CanReleaseCount
        {
            get;
        }

        public abstract bool AllowMultiSpawn
        {
            get;
        }

        public abstract float AutoReleaseInterval
        {
            get;
            set;
        }

        public abstract int Capacity
        {
            get;
            set;
        }

        public abstract float ExpireTime
        {
            get;
            set;
        }

        public abstract int Priority
        {
            get;
            set;
        }

        public abstract void Release();

        public abstract void Release(int toReleaseCount);

        public abstract void ReleaseAllUnused();

        public abstract ObjectInfo[] GetAllObjectInfos();

        internal abstract void Update(float elapseSeconds, float realElapseSeconds);

        internal abstract void Shutdown();
    }
}
