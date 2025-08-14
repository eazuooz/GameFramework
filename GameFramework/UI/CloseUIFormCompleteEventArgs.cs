//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.UI
{
    public sealed class CloseUIFormCompleteEventArgs : GameFrameworkEventArgs
    {
        public CloseUIFormCompleteEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroup = null;
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

        public IUIGroup UIGroup
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static CloseUIFormCompleteEventArgs Create(int serialId, string uiFormAssetName, IUIGroup uiGroup, object userData)
        {
            CloseUIFormCompleteEventArgs closeUIFormCompleteEventArgs = ReferencePool.Acquire<CloseUIFormCompleteEventArgs>();
            closeUIFormCompleteEventArgs.SerialId = serialId;
            closeUIFormCompleteEventArgs.UIFormAssetName = uiFormAssetName;
            closeUIFormCompleteEventArgs.UIGroup = uiGroup;
            closeUIFormCompleteEventArgs.UserData = userData;
            return closeUIFormCompleteEventArgs;
        }

        public override void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroup = null;
            UserData = null;
        }
    }
}
