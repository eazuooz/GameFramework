//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFramework
{
    public static partial class ReferencePool
    {
        private sealed class ReferenceCollection
        {
            public ReferenceCollection(Type referenceType)
            {
                mReferences = new Queue<IReference>();
                mReferenceType = referenceType;
                mUsingReferenceCount = 0;
                mAcquireReferenceCount = 0;
                mReleaseReferenceCount = 0;
                mAddReferenceCount = 0;
                mRemoveReferenceCount = 0;
            }

            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != mReferenceType)
                {
                    throw new GameFrameworkException("Type is invalid.");
                }

                mUsingReferenceCount++;
                mAcquireReferenceCount++;
                lock (mReferences)
                {
                    if (mReferences.Count > 0)
                    {
                        return (T)mReferences.Dequeue();
                    }
                }

                mAddReferenceCount++;
                return new T();
            }

            public IReference Acquire()
            {
                mUsingReferenceCount++;
                mAcquireReferenceCount++;
                lock (mReferences)
                {
                    if (mReferences.Count > 0)
                    {
                        return mReferences.Dequeue();
                    }
                }

                mAddReferenceCount++;
                return (IReference)Activator.CreateInstance(mReferenceType);
            }

            public void Release(IReference reference)
            {
                reference.Clear();
                lock (mReferences)
                {
                    if (mEnableStrictCheck && mReferences.Contains(reference))
                    {
                        throw new GameFrameworkException("The reference has been released.");
                    }

                    mReferences.Enqueue(reference);
                }

                mReleaseReferenceCount++;
                mUsingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != mReferenceType)
                {
                    throw new GameFrameworkException("Type is invalid.");
                }

                lock (mReferences)
                {
                    mAddReferenceCount += count;
                    while (count-- > 0)
                    {
                        mReferences.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (mReferences)
                {
                    mAddReferenceCount += count;
                    while (count-- > 0)
                    {
                        mReferences.Enqueue((IReference)Activator.CreateInstance(mReferenceType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (mReferences)
                {
                    if (count > mReferences.Count)
                    {
                        count = mReferences.Count;
                    }

                    mRemoveReferenceCount += count;
                    while (count-- > 0)
                    {
                        mReferences.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (mReferences)
                {
                    mRemoveReferenceCount += mReferences.Count;
                    mReferences.Clear();
                }
            }

            public Type ReferenceType
            {
                get
                {
                    return mReferenceType;
                }
            }

            public int UnusedReferenceCount
            {
                get
                {
                    return mReferences.Count;
                }
            }

            public int UsingReferenceCount
            {
                get
                {
                    return mUsingReferenceCount;
                }
            }

            public int AcquireReferenceCount
            {
                get
                {
                    return mAcquireReferenceCount;
                }
            }

            public int ReleaseReferenceCount
            {
                get
                {
                    return mReleaseReferenceCount;
                }
            }

            public int AddReferenceCount
            {
                get
                {
                    return mAddReferenceCount;
                }
            }

            public int RemoveReferenceCount
            {
                get
                {
                    return mRemoveReferenceCount;
                }
            }

            private readonly Queue<IReference> mReferences;
            private readonly Type mReferenceType;
            private int mUsingReferenceCount;
            private int mAcquireReferenceCount;
            private int mReleaseReferenceCount;
            private int mAddReferenceCount;
            private int mRemoveReferenceCount;
        }
    }
}
