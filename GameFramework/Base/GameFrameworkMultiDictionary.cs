//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameFramework
{
    public sealed class GameFrameworkMultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, GameFrameworkLinkedListRange<TValue>>>, IEnumerable
    {
        private readonly GameFrameworkLinkedList<TValue> mLinkedList;
        private readonly Dictionary<TKey, GameFrameworkLinkedListRange<TValue>> mDictionary;

        public GameFrameworkMultiDictionary()
        {
            mLinkedList = new GameFrameworkLinkedList<TValue>();
            mDictionary = new Dictionary<TKey, GameFrameworkLinkedListRange<TValue>>();
        }

        public int Count
        {
            get
            {
                return mDictionary.Count;
            }
        }

        public GameFrameworkLinkedListRange<TValue> this[TKey key]
        {
            get
            {
                GameFrameworkLinkedListRange<TValue> range = default(GameFrameworkLinkedListRange<TValue>);
                mDictionary.TryGetValue(key, out range);
                return range;
            }
        }

        public void Clear()
        {
            mDictionary.Clear();
            mLinkedList.Clear();
        }

        public bool Contains(TKey key)
        {
            return mDictionary.ContainsKey(key);
        }

        public bool Contains(TKey key, TValue value)
        {
            GameFrameworkLinkedListRange<TValue> range = default(GameFrameworkLinkedListRange<TValue>);
            if (mDictionary.TryGetValue(key, out range))
            {
                return range.Contains(value);
            }

            return false;
        }

        public bool TryGetValue(TKey key, out GameFrameworkLinkedListRange<TValue> range)
        {
            return mDictionary.TryGetValue(key, out range);
        }

        public void Add(TKey key, TValue value)
        {
            GameFrameworkLinkedListRange<TValue> range = default(GameFrameworkLinkedListRange<TValue>);
            if (mDictionary.TryGetValue(key, out range))
            {
                mLinkedList.AddBefore(range.Terminal, value);
            }
            else
            {
                LinkedListNode<TValue> first = mLinkedList.AddLast(value);
                LinkedListNode<TValue> terminal = mLinkedList.AddLast(default(TValue));
                mDictionary.Add(key, new GameFrameworkLinkedListRange<TValue>(first, terminal));
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            GameFrameworkLinkedListRange<TValue> range = default(GameFrameworkLinkedListRange<TValue>);
            if (mDictionary.TryGetValue(key, out range))
            {
                for (LinkedListNode<TValue> current = range.First; current != null && current != range.Terminal; current = current.Next)
                {
                    if (current.Value.Equals(value))
                    {
                        if (current == range.First)
                        {
                            LinkedListNode<TValue> next = current.Next;
                            if (next == range.Terminal)
                            {
                                mLinkedList.Remove(next);
                                mDictionary.Remove(key);
                            }
                            else
                            {
                                mDictionary[key] = new GameFrameworkLinkedListRange<TValue>(next, range.Terminal);
                            }
                        }

                        mLinkedList.Remove(current);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool RemoveAll(TKey key)
        {
            GameFrameworkLinkedListRange<TValue> range = default(GameFrameworkLinkedListRange<TValue>);
            if (mDictionary.TryGetValue(key, out range))
            {
                mDictionary.Remove(key);

                LinkedListNode<TValue> current = range.First;
                while (current != null)
                {
                    LinkedListNode<TValue> next = current != range.Terminal ? current.Next : null;
                    mLinkedList.Remove(current);
                    current = next;
                }

                return true;
            }

            return false;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(mDictionary);
        }

        IEnumerator<KeyValuePair<TKey, GameFrameworkLinkedListRange<TValue>>> IEnumerable<KeyValuePair<TKey, GameFrameworkLinkedListRange<TValue>>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, GameFrameworkLinkedListRange<TValue>>>, IEnumerator
        {
            private Dictionary<TKey, GameFrameworkLinkedListRange<TValue>>.Enumerator mEnumerator;

            internal Enumerator(Dictionary<TKey, GameFrameworkLinkedListRange<TValue>> dictionary)
            {
                if (dictionary == null)
                {
                    throw new GameFrameworkException("Dictionary is invalid.");
                }

                mEnumerator = dictionary.GetEnumerator();
            }

            public KeyValuePair<TKey, GameFrameworkLinkedListRange<TValue>> Current
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
                ((IEnumerator<KeyValuePair<TKey, GameFrameworkLinkedListRange<TValue>>>)mEnumerator).Reset();
            }
        }
    }
}
