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
    [StructLayout(LayoutKind.Auto)]
    public struct GameFrameworkLinkedListRange<T> : IEnumerable<T>, IEnumerable
    {
        private readonly LinkedListNode<T> mFirst;
        private readonly LinkedListNode<T> mTerminal;

        public GameFrameworkLinkedListRange(LinkedListNode<T> first, LinkedListNode<T> terminal)
        {
            if (first == null || terminal == null || first == terminal)
            {
                throw new GameFrameworkException("Range is invalid.");
            }

            mFirst = first;
            mTerminal = terminal;
        }

        public bool IsValid
        {
            get
            {
                return mFirst != null && mTerminal != null && mFirst != mTerminal;
            }
        }

        public LinkedListNode<T> First
        {
            get
            {
                return mFirst;
            }
        }

        public LinkedListNode<T> Terminal
        {
            get
            {
                return mTerminal;
            }
        }

        public int Count
        {
            get
            {
                if (!IsValid)
                {
                    return 0;
                }

                int count = 0;
                for (LinkedListNode<T> current = mFirst; current != null && current != mTerminal; current = current.Next)
                {
                    count++;
                }

                return count;
            }
        }

        public bool Contains(T value)
        {
            for (LinkedListNode<T> current = mFirst; current != null && current != mTerminal; current = current.Next)
            {
                if (current.Value.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
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
            private readonly GameFrameworkLinkedListRange<T> mGameFrameworkLinkedListRange;
            private LinkedListNode<T> mCurrent;
            private T mCurrentValue;

            internal Enumerator(GameFrameworkLinkedListRange<T> range)
            {
                if (!range.IsValid)
                {
                    throw new GameFrameworkException("Range is invalid.");
                }

                mGameFrameworkLinkedListRange = range;
                mCurrent = mGameFrameworkLinkedListRange.mFirst;
                mCurrentValue = default(T);
            }

            public T Current
            {
                get
                {
                    return mCurrentValue;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return mCurrentValue;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (mCurrent == null || mCurrent == mGameFrameworkLinkedListRange.mTerminal)
                {
                    return false;
                }

                mCurrentValue = mCurrent.Value;
                mCurrent = mCurrent.Next;
                return true;
            }

            void IEnumerator.Reset()
            {
                mCurrent = mGameFrameworkLinkedListRange.mFirst;
                mCurrentValue = default(T);
            }
        }
    }
}
