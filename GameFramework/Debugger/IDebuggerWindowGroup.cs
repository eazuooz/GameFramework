//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Debugger
{
    public interface IDebuggerWindowGroup : IDebuggerWindow
    {
        int DebuggerWindowCount
        {
            get;
        }

        int SelectedIndex
        {
            get;
            set;
        }

        IDebuggerWindow SelectedWindow
        {
            get;
        }

        string[] GetDebuggerWindowNames();

        IDebuggerWindow GetDebuggerWindow(string path);

        void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow);
    }
}
