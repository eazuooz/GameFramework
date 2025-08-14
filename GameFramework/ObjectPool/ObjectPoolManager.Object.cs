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
    internal sealed partial class ObjectPoolManager : GameFrameworkModule, IObjectPoolManager
    {
        private sealed class Object<T> : IReference where T : ObjectBase
        {
            private T mObject;
            private int mSpawnCount;

            public Object()
            {
                mObject = null;
                mSpawnCount = 0;
            }

            public string Name
            {
                get
                {
                    return mObject.Name;
                }
            }

            public bool Locked
            {
                get
                {
                    return mObject.Locked;
                }
                internal set
                {
                    mObject.Locked = value;
                }
            }

            public int Priority
            {
                get
                {
                    return mObject.Priority;
                }
                internal set
                {
                    mObject.Priority = value;
                }
            }

            public bool CustomCanReleaseFlag
            {
                get
                {
                    return mObject.CustomCanReleaseFlag;
                }
            }

            public DateTime LastUseTime
            {
                get
                {
                    return mObject.LastUseTime;
                }
            }

            public bool IsInUse
            {
                get
                {
                    return mSpawnCount > 0;
                }
            }

            public int SpawnCount
            {
                get
                {
                    return mSpawnCount;
                }
            }

            public static Object<T> Create(T obj, bool spawned)
            {
                if (obj == null)
                {
                    throw new GameFrameworkException("Object is invalid.");
                }

                Object<T> internalObject = ReferencePool.Acquire<Object<T>>();
                internalObject.mObject = obj;
                internalObject.mSpawnCount = spawned ? 1 : 0;
                if (spawned)
                {
                    obj.OnSpawn();
                }

                return internalObject;
            }

            public void Clear()
            {
                mObject = null;
                mSpawnCount = 0;
            }

            public T Peek()
            {
                return mObject;
            }

            public T Spawn()
            {
                mSpawnCount++;
                mObject.LastUseTime = DateTime.UtcNow;
                mObject.OnSpawn();
                return mObject;
            }

            public void Unspawn()
            {
                mObject.OnUnspawn();
                mObject.LastUseTime = DateTime.UtcNow;
                mSpawnCount--;
                if (mSpawnCount < 0)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Object '{0}' spawn count is less than 0.", Name));
                }
            }

            public void Release(bool isShutdown)
            {
                mObject.Release(isShutdown);
                ReferencePool.Release(mObject);
            }
        }
    }
}
