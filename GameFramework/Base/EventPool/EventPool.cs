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
    internal sealed partial class EventPool<T> where T : BaseEventArgs
    {
        public EventPool(EventPoolMode mode)
        {
            mEventHandlers = new GameFrameworkMultiDictionary<int, EventHandler<T>>();
            mEvents = new Queue<Event>();
            mCachedNodes = new Dictionary<object, LinkedListNode<EventHandler<T>>>();
            mTempNodes = new Dictionary<object, LinkedListNode<EventHandler<T>>>();
            mEventPoolMode = mode;
            mDefaultHandler = null;
        }


        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (mEvents)
            {
                while (mEvents.Count > 0)
                {
                    Event eventNode = mEvents.Dequeue();
                    HandleEvent(eventNode.Sender, eventNode.EventArgs);
                    ReferencePool.Release(eventNode);
                }
            }
        }

        public void Shutdown()
        {
            Clear();
            mEventHandlers.Clear();
            mCachedNodes.Clear();
            mTempNodes.Clear();
            mDefaultHandler = null;
        }

        public void Clear()
        {
            lock (mEvents)
            {
                mEvents.Clear();
            }
        }

        public int Count(int id)
        {
            GameFrameworkLinkedListRange<EventHandler<T>> range = default(GameFrameworkLinkedListRange<EventHandler<T>>);
            if (mEventHandlers.TryGetValue(id, out range))
            {
                return range.Count;
            }

            return 0;
        }

        public bool Check(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new GameFrameworkException("Event handler is invalid.");
            }

            return mEventHandlers.Contains(id, handler);
        }

        public void Subscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new GameFrameworkException("Event handler is invalid.");
            }

            if (!mEventHandlers.Contains(id))
            {
                mEventHandlers.Add(id, handler);
            }
            else if ((mEventPoolMode & EventPoolMode.AllowMultiHandler) != EventPoolMode.AllowMultiHandler)
            {
                throw new GameFrameworkException(Utility.Text.Format("Event '{0}' not allow multi handler.", id));
            }
            else if ((mEventPoolMode & EventPoolMode.AllowDuplicateHandler) != EventPoolMode.AllowDuplicateHandler && Check(id, handler))
            {
                throw new GameFrameworkException(Utility.Text.Format("Event '{0}' not allow duplicate handler.", id));
            }
            else
            {
                mEventHandlers.Add(id, handler);
            }
        }

        public void Unsubscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new GameFrameworkException("Event handler is invalid.");
            }

            if (mCachedNodes.Count > 0)
            {
                foreach (KeyValuePair<object, LinkedListNode<EventHandler<T>>> cachedNode in mCachedNodes)
                {
                    if (cachedNode.Value != null && cachedNode.Value.Value == handler)
                    {
                        mTempNodes.Add(cachedNode.Key, cachedNode.Value.Next);
                    }
                }

                if (mTempNodes.Count > 0)
                {
                    foreach (KeyValuePair<object, LinkedListNode<EventHandler<T>>> cachedNode in mTempNodes)
                    {
                        mCachedNodes[cachedNode.Key] = cachedNode.Value;
                    }

                    mTempNodes.Clear();
                }
            }

            if (!mEventHandlers.Remove(id, handler))
            {
                throw new GameFrameworkException(Utility.Text.Format("Event '{0}' not exists specified handler.", id));
            }
        }

        public void SetDefaultHandler(EventHandler<T> handler)
        {
            mDefaultHandler = handler;
        }

        public void Fire(object sender, T e)
        {
            if (e == null)
            {
                throw new GameFrameworkException("Event is invalid.");
            }

            Event eventNode = Event.Create(sender, e);
            lock (mEvents)
            {
                mEvents.Enqueue(eventNode);
            }
        }

        public void FireNow(object sender, T e)
        {
            if (e == null)
            {
                throw new GameFrameworkException("Event is invalid.");
            }

            HandleEvent(sender, e);
        }

        private void HandleEvent(object sender, T e)
        {
            bool noHandlerException = false;
            GameFrameworkLinkedListRange<EventHandler<T>> range = default(GameFrameworkLinkedListRange<EventHandler<T>>);
            if (mEventHandlers.TryGetValue(e.Id, out range))
            {
                LinkedListNode<EventHandler<T>> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    mCachedNodes[e] = current.Next != range.Terminal ? current.Next : null;
                    current.Value(sender, e);
                    current = mCachedNodes[e];
                }

                mCachedNodes.Remove(e);
            }
            else if (mDefaultHandler != null)
            {
                mDefaultHandler(sender, e);
            }
            else if ((mEventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            ReferencePool.Release(e);

            if (noHandlerException)
            {
                throw new GameFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", e.Id));
            }
        }

        public int EventHandlerCount
        {
            get
            {
                return mEventHandlers.Count;
            }
        }

        public int EventCount
        {
            get
            {
                return mEvents.Count;
            }
        }

        private readonly GameFrameworkMultiDictionary<int, EventHandler<T>> mEventHandlers;
        private readonly Queue<Event> mEvents;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> mCachedNodes;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> mTempNodes;
        private readonly EventPoolMode mEventPoolMode;
        private EventHandler<T> mDefaultHandler;
    }
}