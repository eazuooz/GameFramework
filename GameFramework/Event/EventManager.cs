//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework.Event
{
    internal sealed class EventManager : GameFrameworkModule, IEventManager
    {
        private readonly EventPool<GameEventArgs> mEventPool;

        public EventManager()
        {
            mEventPool = new EventPool<GameEventArgs>(EventPoolMode.AllowNoHandler | EventPoolMode.AllowMultiHandler);
        }

        public int EventHandlerCount
        {
            get
            {
                return mEventPool.EventHandlerCount;
            }
        }

        public int EventCount
        {
            get
            {
                return mEventPool.EventCount;
            }
        }

        internal override int Priority
        {
            get
            {
                return 7;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            mEventPool.Update(elapseSeconds, realElapseSeconds);
        }

        internal override void Shutdown()
        {
            mEventPool.Shutdown();
        }

        public int Count(int id)
        {
            return mEventPool.Count(id);
        }

        public bool Check(int id, EventHandler<GameEventArgs> handler)
        {
            return mEventPool.Check(id, handler);
        }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            mEventPool.Subscribe(id, handler);
        }

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            mEventPool.Unsubscribe(id, handler);
        }

        public void SetDefaultHandler(EventHandler<GameEventArgs> handler)
        {
            mEventPool.SetDefaultHandler(handler);
        }

        public void Fire(object sender, GameEventArgs e)
        {
            mEventPool.Fire(sender, e);
        }

        public void FireNow(object sender, GameEventArgs e)
        {
            mEventPool.FireNow(sender, e);
        }
    }
}
