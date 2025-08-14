//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameFramework
{
    public sealed class GameFrameworkLinkedList<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private readonly LinkedList<T> mLinkedList;
        private readonly Queue<LinkedListNode<T>> mCachedNodes;

        public GameFrameworkLinkedList()
        {
            mLinkedList = new LinkedList<T>();
            mCachedNodes = new Queue<LinkedListNode<T>>();
        }

        public int Count
        {
            get
            {
                return mLinkedList.Count;
            }
        }

        public int CachedNodeCount
        {
            get
            {
                return mCachedNodes.Count;
            }
        }

        public LinkedListNode<T> First
        {
            get
            {
                return mLinkedList.First;
            }
        }

        public LinkedListNode<T> Last
        {
            get
            {
                return mLinkedList.Last;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((ICollection<T>)mLinkedList).IsReadOnly;
            }
        }

        public object SyncRoot
        {
            get
            {
                return ((ICollection)mLinkedList).SyncRoot;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return ((ICollection)mLinkedList).IsSynchronized;
            }
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> newNode = AcquireNode(value);
            mLinkedList.AddAfter(node, newNode);
            return newNode;
        }

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            mLinkedList.AddAfter(node, newNode);
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> newNode = AcquireNode(value);
            mLinkedList.AddBefore(node, newNode);
            return newNode;
        }

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            mLinkedList.AddBefore(node, newNode);
        }

        public LinkedListNode<T> AddFirst(T value)
        {
            LinkedListNode<T> node = AcquireNode(value);
            mLinkedList.AddFirst(node);
            return node;
        }

        public void AddFirst(LinkedListNode<T> node)
        {
            mLinkedList.AddFirst(node);
        }

        public LinkedListNode<T> AddLast(T value)
        {
            LinkedListNode<T> node = AcquireNode(value);
            mLinkedList.AddLast(node);
            return node;
        }

        public void AddLast(LinkedListNode<T> node)
        {
            mLinkedList.AddLast(node);
        }

        public void Clear()
        {
            LinkedListNode<T> current = mLinkedList.First;
            while (current != null)
            {
                ReleaseNode(current);
                current = current.Next;
            }

            mLinkedList.Clear();
        }

        public void ClearCachedNodes()
        {
            mCachedNodes.Clear();
        }

        public bool Contains(T value)
        {
            return mLinkedList.Contains(value);
        }

        public void CopyTo(T[] array, int index)
        {
            mLinkedList.CopyTo(array, index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)mLinkedList).CopyTo(array, index);
        }

        public LinkedListNode<T> Find(T value)
        {
            return mLinkedList.Find(value);
        }

        public LinkedListNode<T> FindLast(T value)
        {
            return mLinkedList.FindLast(value);
        }

        public bool Remove(T value)
        {
            LinkedListNode<T> node = mLinkedList.Find(value);
            if (node != null)
            {
                mLinkedList.Remove(node);
                ReleaseNode(node);
                return true;
            }

            return false;
        }

        public void Remove(LinkedListNode<T> node)
        {
            mLinkedList.Remove(node);
            ReleaseNode(node);
        }

        public void RemoveFirst()
        {
            LinkedListNode<T> first = mLinkedList.First;
            if (first == null)
            {
                throw new GameFrameworkException("First is invalid.");
            }

            mLinkedList.RemoveFirst();
            ReleaseNode(first);
        }

        public void RemoveLast()
        {
            LinkedListNode<T> last = mLinkedList.Last;
            if (last == null)
            {
                throw new GameFrameworkException("Last is invalid.");
            }

            mLinkedList.RemoveLast();
            ReleaseNode(last);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(mLinkedList);
        }

        private LinkedListNode<T> AcquireNode(T value)
        {
            LinkedListNode<T> node = null;
            if (mCachedNodes.Count > 0)
            {
                node = mCachedNodes.Dequeue();
                node.Value = value;
            }
            else
            {
                node = new LinkedListNode<T>(value);
            }

            return node;
        }

        private void ReleaseNode(LinkedListNode<T> node)
        {
            node.Value = default(T);
            mCachedNodes.Enqueue(node);
        }

        void ICollection<T>.Add(T value)
        {
            AddLast(value);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private LinkedList<T>.Enumerator mEnumerator;

            internal Enumerator(LinkedList<T> linkedList)
            {
                if (linkedList == null)
                {
                    throw new GameFrameworkException("Linked list is invalid.");
                }

                mEnumerator = linkedList.GetEnumerator();
            }

            public T Current
            {
                get
                {
                    return mEnumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return mEnumerator.Current;
                }
            }

            public void Dispose()
            {
                mEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return mEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                ((IEnumerator<T>)mEnumerator).Reset();
            }
        }
    }
}
