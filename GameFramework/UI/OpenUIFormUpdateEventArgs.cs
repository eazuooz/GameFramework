//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.UI
{
    public sealed class OpenUIFormUpdateEventArgs : GameFrameworkEventArgs
    {
        public OpenUIFormUpdateEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            Progress = 0f;
            UserData = null;
        }

        public int SerialId
        {
            get;
            private set;
        }

        public string UIFormAssetName
        {
            get;
            private set;
        }

        public string UIGroupName
        {
            get;
            private set;
        }

        public bool PauseCoveredUIForm
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

        public static OpenUIFormUpdateEventArgs Create(int serialId, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, float progress, object userData)
        {
            OpenUIFormUpdateEventArgs openUIFormUpdateEventArgs = ReferencePool.Acquire<OpenUIFormUpdateEventArgs>();
            openUIFormUpdateEventArgs.SerialId = serialId;
            openUIFormUpdateEventArgs.UIFormAssetName = uiFormAssetName;
            openUIFormUpdateEventArgs.UIGroupName = uiGroupName;
            openUIFormUpdateEventArgs.PauseCoveredUIForm = pauseCoveredUIForm;
            openUIFormUpdateEventArgs.Progress = progress;
            openUIFormUpdateEventArgs.UserData = userData;
            return openUIFormUpdateEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            Progress = 0f;
            UserData = null;
        }
    }
}
