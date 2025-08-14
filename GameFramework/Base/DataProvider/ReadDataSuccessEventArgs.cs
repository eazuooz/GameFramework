//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    public sealed class ReadDataSuccessEventArgs : GameFrameworkEventArgs
    {
        public ReadDataSuccessEventArgs()
        {
            DataAssetName = null;
            Duration = 0f;
            UserData = null;
        }

        public static ReadDataSuccessEventArgs Create(string dataAssetName, float duration, object userData)
        {
            ReadDataSuccessEventArgs loadDataSuccessEventArgs = ReferencePool.Acquire<ReadDataSuccessEventArgs>();
            loadDataSuccessEventArgs.DataAssetName = dataAssetName;
            loadDataSuccessEventArgs.Duration = duration;
            loadDataSuccessEventArgs.UserData = userData;
            return loadDataSuccessEventArgs;
        }

        public override void Clear()
        {
            DataAssetName = null;
            Duration = 0f;
            UserData = null;
        }

        public string DataAssetName
        {
            get;
            private set;
        }

        public float Duration
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