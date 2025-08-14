//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    public sealed class ReadDataFailureEventArgs : GameFrameworkEventArgs
    {
        public ReadDataFailureEventArgs()
        {
            DataAssetName = null;
            ErrorMessage = null;
            UserData = null;
        }

        public static ReadDataFailureEventArgs Create(string dataAssetName, string errorMessage, object userData)
        {
            ReadDataFailureEventArgs loadDataFailureEventArgs = ReferencePool.Acquire<ReadDataFailureEventArgs>();
            loadDataFailureEventArgs.DataAssetName = dataAssetName;
            loadDataFailureEventArgs.ErrorMessage = errorMessage;
            loadDataFailureEventArgs.UserData = userData;
            return loadDataFailureEventArgs;
        }

        public override void Clear()
        {
            DataAssetName = null;
            ErrorMessage = null;
            UserData = null;
        }

        public string DataAssetName
        {
            get;
            private set;
        }

        public string ErrorMessage
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