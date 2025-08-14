//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework.ObjectPool
{
    internal sealed partial class ObjectPoolManager : GameFrameworkModule, IObjectPoolManager
    {
        private sealed class ObjectPool<T> : ObjectPoolBase, IObjectPool<T> where T : ObjectBase
        {
            private readonly GameFrameworkMultiDictionary<string, Object<T>> mObjects;
            private readonly Dictionary<object, Object<T>> mObjectMap;
            private readonly ReleaseObjectFilterCallback<T> mDefaultReleaseObjectFilterCallback;
            private readonly List<T> mCachedCanReleaseObjects;
            private readonly List<T> mCachedToReleaseObjects;
            private readonly bool mAllowMultiSpawn;
            private float mAutoReleaseInterval;
            private int mCapacity;
            private float mExpireTime;
            private int mPriority;
            private float mAutoReleaseTime;

            public ObjectPool(string name, bool allowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority)
                : base(name)
            {
                mObjects = new GameFrameworkMultiDictionary<string, Object<T>>();
                mObjectMap = new Dictionary<object, Object<T>>();
                mDefaultReleaseObjectFilterCallback = DefaultReleaseObjectFilterCallback;
                mCachedCanReleaseObjects = new List<T>();
                mCachedToReleaseObjects = new List<T>();
                mAllowMultiSpawn = allowMultiSpawn;
                mAutoReleaseInterval = autoReleaseInterval;
                Capacity = capacity;
                ExpireTime = expireTime;
                mPriority = priority;
                mAutoReleaseTime = 0f;
            }

            public override Type ObjectType
            {
                get
                {
                    return typeof(T);
                }
            }

            public override int Count
            {
                get
                {
                    return mObjectMap.Count;
                }
            }

            public override int CanReleaseCount
            {
                get
                {
                    GetCanReleaseObjects(mCachedCanReleaseObjects);
                    return mCachedCanReleaseObjects.Count;
                }
            }

            public override bool AllowMultiSpawn
            {
                get
                {
                    return mAllowMultiSpawn;
                }
            }

            public override float AutoReleaseInterval
            {
                get
                {
                    return mAutoReleaseInterval;
                }
                set
                {
                    mAutoReleaseInterval = value;
                }
            }

            public override int Capacity
            {
                get
                {
                    return mCapacity;
                }
                set
                {
                    if (value < 0)
                    {
                        throw new GameFrameworkException("Capacity is invalid.");
                    }

                    if (mCapacity == value)
                    {
                        return;
                    }

                    mCapacity = value;
                    Release();
                }
            }

            public override float ExpireTime
            {
                get
                {
                    return mExpireTime;
                }

                set
                {
                    if (value < 0f)
                    {
                        throw new GameFrameworkException("ExpireTime is invalid.");
                    }

                    if (ExpireTime == value)
                    {
                        return;
                    }

                    mExpireTime = value;
                    Release();
                }
            }

            public override int Priority
            {
                get
                {
                    return mPriority;
                }
                set
                {
                    mPriority = value;
                }
            }

            public void Register(T obj, bool spawned)
            {
                if (obj == null)
                {
                    throw new GameFrameworkException("Object is invalid.");
                }

                Object<T> internalObject = Object<T>.Create(obj, spawned);
                mObjects.Add(obj.Name, internalObject);
                mObjectMap.Add(obj.Target, internalObject);

                if (Count > mCapacity)
                {
                    Release();
                }
            }

            public bool CanSpawn()
            {
                return CanSpawn(string.Empty);
            }

            public bool CanSpawn(string name)
            {
                if (name == null)
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                GameFrameworkLinkedListRange<Object<T>> objectRange = default(GameFrameworkLinkedListRange<Object<T>>);
                if (mObjects.TryGetValue(name, out objectRange))
                {
                    foreach (Object<T> internalObject in objectRange)
                    {
                        if (mAllowMultiSpawn || !internalObject.IsInUse)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public T Spawn()
            {
                return Spawn(string.Empty);
            }

            public T Spawn(string name)
            {
                if (name == null)
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                GameFrameworkLinkedListRange<Object<T>> objectRange = default(GameFrameworkLinkedListRange<Object<T>>);
                if (mObjects.TryGetValue(name, out objectRange))
                {
                    foreach (Object<T> internalObject in objectRange)
                    {
                        if (mAllowMultiSpawn || !internalObject.IsInUse)
                        {
                            return internalObject.Spawn();
                        }
                    }
                }

                return null;
            }

            public void Unspawn(T obj)
            {
                if (obj == null)
                {
                    throw new GameFrameworkException("Object is invalid.");
                }

                Unspawn(obj.Target);
            }

            public void Unspawn(object target)
            {
                if (target == null)
                {
                    throw new GameFrameworkException("Target is invalid.");
                }

                Object<T> internalObject = GetObject(target);
                if (internalObject != null)
                {
                    internalObject.Unspawn();
                    if (Count > mCapacity && internalObject.SpawnCount <= 0)
                    {
                        Release();
                    }
                }
                else
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not find target in object pool '{0}', target type is '{1}', target value is '{2}'.", new TypeNamePair(typeof(T), Name), target.GetType().FullName, target));
                }
            }

            public void SetLocked(T obj, bool locked)
            {
                if (obj == null)
                {
                    throw new GameFrameworkException("Object is invalid.");
                }

                SetLocked(obj.Target, locked);
            }

            public void SetLocked(object target, bool locked)
            {
                if (target == null)
                {
                    throw new GameFrameworkException("Target is invalid.");
                }

                Object<T> internalObject = GetObject(target);
                if (internalObject != null)
                {
                    internalObject.Locked = locked;
                }
                else
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not find target in object pool '{0}', target type is '{1}', target value is '{2}'.", new TypeNamePair(typeof(T), Name), target.GetType().FullName, target));
                }
            }

            public void SetPriority(T obj, int priority)
            {
                if (obj == null)
                {
                    throw new GameFrameworkException("Object is invalid.");
                }

                SetPriority(obj.Target, priority);
            }

            public void SetPriority(object target, int priority)
            {
                if (target == null)
                {
                    throw new GameFrameworkException("Target is invalid.");
                }

                Object<T> internalObject = GetObject(target);
                if (internalObject != null)
                {
                    internalObject.Priority = priority;
                }
                else
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not find target in object pool '{0}', target type is '{1}', target value is '{2}'.", new TypeNamePair(typeof(T), Name), target.GetType().FullName, target));
                }
            }

            public bool ReleaseObject(T obj)
            {
                if (obj == null)
                {
                    throw new GameFrameworkException("Object is invalid.");
                }

                return ReleaseObject(obj.Target);
            }

            public bool ReleaseObject(object target)
            {
                if (target == null)
                {
                    throw new GameFrameworkException("Target is invalid.");
                }

                Object<T> internalObject = GetObject(target);
                if (internalObject == null)
                {
                    return false;
                }

                if (internalObject.IsInUse || internalObject.Locked || !internalObject.CustomCanReleaseFlag)
                {
                    return false;
                }

                mObjects.Remove(internalObject.Name, internalObject);
                mObjectMap.Remove(internalObject.Peek().Target);

                internalObject.Release(false);
                ReferencePool.Release(internalObject);
                return true;
            }

            public override void Release()
            {
                Release(Count - mCapacity, mDefaultReleaseObjectFilterCallback);
            }

            public override void Release(int toReleaseCount)
            {
                Release(toReleaseCount, mDefaultReleaseObjectFilterCallback);
            }

            public void Release(ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
            {
                Release(Count - mCapacity, releaseObjectFilterCallback);
            }

            public void Release(int toReleaseCount, ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
            {
                if (releaseObjectFilterCallback == null)
                {
                    throw new GameFrameworkException("Release object filter callback is invalid.");
                }

                if (toReleaseCount < 0)
                {
                    toReleaseCount = 0;
                }

                DateTime expireTime = DateTime.MinValue;
                if (mExpireTime < float.MaxValue)
                {
                    expireTime = DateTime.UtcNow.AddSeconds(-mExpireTime);
                }

                mAutoReleaseTime = 0f;
                GetCanReleaseObjects(mCachedCanReleaseObjects);
                List<T> toReleaseObjects = releaseObjectFilterCallback(mCachedCanReleaseObjects, toReleaseCount, expireTime);
                if (toReleaseObjects == null || toReleaseObjects.Count <= 0)
                {
                    return;
                }

                foreach (T toReleaseObject in toReleaseObjects)
                {
                    ReleaseObject(toReleaseObject);
                }
            }

            public override void ReleaseAllUnused()
            {
                mAutoReleaseTime = 0f;
                GetCanReleaseObjects(mCachedCanReleaseObjects);
                foreach (T toReleaseObject in mCachedCanReleaseObjects)
                {
                    ReleaseObject(toReleaseObject);
                }
            }

            public override ObjectInfo[] GetAllObjectInfos()
            {
                List<ObjectInfo> results = new List<ObjectInfo>();
                foreach (KeyValuePair<string, GameFrameworkLinkedListRange<Object<T>>> objectRanges in mObjects)
                {
                    foreach (Object<T> internalObject in objectRanges.Value)
                    {
                        results.Add(new ObjectInfo(internalObject.Name, internalObject.Locked, internalObject.CustomCanReleaseFlag, internalObject.Priority, internalObject.LastUseTime, internalObject.SpawnCount));
                    }
                }

                return results.ToArray();
            }

            internal override void Update(float elapseSeconds, float realElapseSeconds)
            {
                mAutoReleaseTime += realElapseSeconds;
                if (mAutoReleaseTime < mAutoReleaseInterval)
                {
                    return;
                }

                Release();
            }

            internal override void Shutdown()
            {
                foreach (KeyValuePair<object, Object<T>> objectInMap in mObjectMap)
                {
                    objectInMap.Value.Release(true);
                    ReferencePool.Release(objectInMap.Value);
                }

                mObjects.Clear();
                mObjectMap.Clear();
                mCachedCanReleaseObjects.Clear();
                mCachedToReleaseObjects.Clear();
            }

            private Object<T> GetObject(object target)
            {
                if (target == null)
                {
                    throw new GameFrameworkException("Target is invalid.");
                }

                Object<T> internalObject = null;
                if (mObjectMap.TryGetValue(target, out internalObject))
                {
                    return internalObject;
                }

                return null;
            }

            private void GetCanReleaseObjects(List<T> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (KeyValuePair<object, Object<T>> objectInMap in mObjectMap)
                {
                    Object<T> internalObject = objectInMap.Value;
                    if (internalObject.IsInUse || internalObject.Locked || !internalObject.CustomCanReleaseFlag)
                    {
                        continue;
                    }

                    results.Add(internalObject.Peek());
                }
            }

            private List<T> DefaultReleaseObjectFilterCallback(List<T> candidateObjects, int toReleaseCount, DateTime expireTime)
            {
                mCachedToReleaseObjects.Clear();

                if (expireTime > DateTime.MinValue)
                {
                    for (int i = candidateObjects.Count - 1; i >= 0; i--)
                    {
                        if (candidateObjects[i].LastUseTime <= expireTime)
                        {
                            mCachedToReleaseObjects.Add(candidateObjects[i]);
                            candidateObjects.RemoveAt(i);
                            continue;
                        }
                    }

                    toReleaseCount -= mCachedToReleaseObjects.Count;
                }

                for (int i = 0; toReleaseCount > 0 && i < candidateObjects.Count; i++)
                {
                    for (int j = i + 1; j < candidateObjects.Count; j++)
                    {
                        if (candidateObjects[i].Priority > candidateObjects[j].Priority
                            || candidateObjects[i].Priority == candidateObjects[j].Priority && candidateObjects[i].LastUseTime > candidateObjects[j].LastUseTime)
                        {
                            T temp = candidateObjects[i];
                            candidateObjects[i] = candidateObjects[j];
                            candidateObjects[j] = temp;
                        }
                    }

                    mCachedToReleaseObjects.Add(candidateObjects[i]);
                    toReleaseCount--;
                }

                return mCachedToReleaseObjects;
            }
        }
    }
}
