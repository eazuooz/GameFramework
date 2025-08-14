//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    internal sealed partial class EventPool<T> where T : BaseEventArgs
    {
        private sealed class Event : IReference
        {
            public static Event Create(object sender, T e)
            {
                Event eventNode = ReferencePool.Acquire<Event>();
                eventNode.mSender = sender;
                eventNode.mEventArgs = e;
                return eventNode;
            }

            public void Clear()
            {
                mSender = null;
                mEventArgs = null;
            }

            public Event()
            {
                mSender = null;
                mEventArgs = null;
            }

            public object Sender
            {
                get
                {
                    return mSender;
                }
            }

            public T EventArgs
            {
                get
                {
                    return mEventArgs;
                }
            }

            private object mSender;
            private T mEventArgs;
        }
    }
}