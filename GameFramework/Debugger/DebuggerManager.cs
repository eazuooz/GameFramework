//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Debugger
{
    internal sealed partial class DebuggerManager : GameFrameworkModule, IDebuggerManager
    {
        private readonly DebuggerWindowGroup mDebuggerWindowRoot;
        private bool mActiveWindow;

        public DebuggerManager()
        {
            mDebuggerWindowRoot = new DebuggerWindowGroup();
            mActiveWindow = false;
        }

        internal override int Priority
        {
            get
            {
                return -1;
            }
        }

        public bool ActiveWindow
        {
            get
            {
                return mActiveWindow;
            }
            set
            {
                mActiveWindow = value;
            }
        }

        public IDebuggerWindowGroup DebuggerWindowRoot
        {
            get
            {
                return mDebuggerWindowRoot;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!mActiveWindow)
            {
                return;
            }

            mDebuggerWindowRoot.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        internal override void Shutdown()
        {
            mActiveWindow = false;
            mDebuggerWindowRoot.Shutdown();
        }

        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new GameFrameworkException("Path is invalid.");
            }

            if (debuggerWindow == null)
            {
                throw new GameFrameworkException("Debugger window is invalid.");
            }

            mDebuggerWindowRoot.RegisterDebuggerWindow(path, debuggerWindow);
            debuggerWindow.Initialize(args);
        }

        public bool UnregisterDebuggerWindow(string path)
        {
            return mDebuggerWindowRoot.UnregisterDebuggerWindow(path);
        }

        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return mDebuggerWindowRoot.GetDebuggerWindow(path);
        }

        public bool SelectDebuggerWindow(string path)
        {
            return mDebuggerWindowRoot.SelectDebuggerWindow(path);
        }
    }
}
