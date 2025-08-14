//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.Debugger
{
    internal sealed partial class DebuggerManager : GameFrameworkModule, IDebuggerManager
    {
        private sealed class DebuggerWindowGroup : IDebuggerWindowGroup
        {
            private readonly List<KeyValuePair<string, IDebuggerWindow>> mDebuggerWindows;
            private int mSelectedIndex;
            private string[] mDebuggerWindowNames;

            public DebuggerWindowGroup()
            {
                mDebuggerWindows = new List<KeyValuePair<string, IDebuggerWindow>>();
                mSelectedIndex = 0;
                mDebuggerWindowNames = null;
            }

            public int DebuggerWindowCount
            {
                get
                {
                    return mDebuggerWindows.Count;
                }
            }

            public int SelectedIndex
            {
                get
                {
                    return mSelectedIndex;
                }
                set
                {
                    mSelectedIndex = value;
                }
            }

            public IDebuggerWindow SelectedWindow
            {
                get
                {
                    if (mSelectedIndex >= mDebuggerWindows.Count)
                    {
                        return null;
                    }

                    return mDebuggerWindows[mSelectedIndex].Value;
                }
            }

            public void Initialize(params object[] args)
            {
            }

            public void Shutdown()
            {
                foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in mDebuggerWindows)
                {
                    debuggerWindow.Value.Shutdown();
                }

                mDebuggerWindows.Clear();
            }

            public void OnEnter()
            {
                SelectedWindow.OnEnter();
            }

            public void OnLeave()
            {
                SelectedWindow.OnLeave();
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                SelectedWindow.OnUpdate(elapseSeconds, realElapseSeconds);
            }

            public void OnDraw()
            {
            }

            private void RefreshDebuggerWindowNames()
            {
                int index = 0;
                mDebuggerWindowNames = new string[mDebuggerWindows.Count];
                foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in mDebuggerWindows)
                {
                    mDebuggerWindowNames[index++] = debuggerWindow.Key;
                }
            }

            public string[] GetDebuggerWindowNames()
            {
                return mDebuggerWindowNames;
            }

            public IDebuggerWindow GetDebuggerWindow(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                int pos = path.IndexOf('/');
                if (pos < 0 || pos >= path.Length - 1)
                {
                    return InternalGetDebuggerWindow(path);
                }

                string debuggerWindowGroupName = path.Substring(0, pos);
                string leftPath = path.Substring(pos + 1);
                DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
                if (debuggerWindowGroup == null)
                {
                    return null;
                }

                return debuggerWindowGroup.GetDebuggerWindow(leftPath);
            }

            public bool SelectDebuggerWindow(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }

                int pos = path.IndexOf('/');
                if (pos < 0 || pos >= path.Length - 1)
                {
                    return InternalSelectDebuggerWindow(path);
                }

                string debuggerWindowGroupName = path.Substring(0, pos);
                string leftPath = path.Substring(pos + 1);
                DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
                if (debuggerWindowGroup == null || !InternalSelectDebuggerWindow(debuggerWindowGroupName))
                {
                    return false;
                }

                return debuggerWindowGroup.SelectDebuggerWindow(leftPath);
            }

            public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new GameFrameworkException("Path is invalid.");
                }

                int pos = path.IndexOf('/');
                if (pos < 0 || pos >= path.Length - 1)
                {
                    if (InternalGetDebuggerWindow(path) != null)
                    {
                        throw new GameFrameworkException("Debugger window has been registered.");
                    }

                    mDebuggerWindows.Add(new KeyValuePair<string, IDebuggerWindow>(path, debuggerWindow));
                    RefreshDebuggerWindowNames();
                }
                else
                {
                    string debuggerWindowGroupName = path.Substring(0, pos);
                    string leftPath = path.Substring(pos + 1);
                    DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
                    if (debuggerWindowGroup == null)
                    {
                        if (InternalGetDebuggerWindow(debuggerWindowGroupName) != null)
                        {
                            throw new GameFrameworkException("Debugger window has been registered, can not create debugger window group.");
                        }

                        debuggerWindowGroup = new DebuggerWindowGroup();
                        mDebuggerWindows.Add(new KeyValuePair<string, IDebuggerWindow>(debuggerWindowGroupName, debuggerWindowGroup));
                        RefreshDebuggerWindowNames();
                    }

                    debuggerWindowGroup.RegisterDebuggerWindow(leftPath, debuggerWindow);
                }
            }

            public bool UnregisterDebuggerWindow(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }

                int pos = path.IndexOf('/');
                if (pos < 0 || pos >= path.Length - 1)
                {
                    IDebuggerWindow debuggerWindow = InternalGetDebuggerWindow(path);
                    bool result = mDebuggerWindows.Remove(new KeyValuePair<string, IDebuggerWindow>(path, debuggerWindow));
                    debuggerWindow.Shutdown();
                    RefreshDebuggerWindowNames();
                    return result;
                }

                string debuggerWindowGroupName = path.Substring(0, pos);
                string leftPath = path.Substring(pos + 1);
                DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
                if (debuggerWindowGroup == null)
                {
                    return false;
                }

                return debuggerWindowGroup.UnregisterDebuggerWindow(leftPath);
            }

            private IDebuggerWindow InternalGetDebuggerWindow(string name)
            {
                foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in mDebuggerWindows)
                {
                    if (debuggerWindow.Key == name)
                    {
                        return debuggerWindow.Value;
                    }
                }

                return null;
            }

            private bool InternalSelectDebuggerWindow(string name)
            {
                for (int i = 0; i < mDebuggerWindows.Count; i++)
                {
                    if (mDebuggerWindows[i].Key == name)
                    {
                        mSelectedIndex = i;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
