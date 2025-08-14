//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    public sealed class ReadDataUpdateEventArgs : GameFrameworkEventArgs
    {
        public ReadDataUpdateEventArgs()
        {
            DataAssetName = null;
            Progress = 0f;
            UserData = null;
        }

        public static ReadDataUpdateEventArgs Create(string dataAssetName, float progress, object userData)
        {
            ReadDataUpdateEventArgs loadDataUpdateEventArgs = ReferencePool.Acquire<ReadDataUpdateEventArgs>();
            loadDataUpdateEventArgs.DataAssetName = dataAssetName;
            loadDataUpdateEventArgs.Progress = progress;
            loadDataUpdateEventArgs.UserData = userData;
            return loadDataUpdateEventArgs;
        }

        public override void Clear()
        {
            DataAssetName = null;
            Progress = 0f;
            UserData = null;
        }

        public string DataAssetName
        {
            get;
            private set;
        }

        public float Progress
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }
    }
}