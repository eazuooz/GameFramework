//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.UI
{
    internal sealed partial class UIManager : GameFrameworkModule, IUIManager
    {
        private sealed class OpenUIFormInfo : IReference
        {
            private int mSerialId;
            private UIGroup mUIGroup;
            private bool mPauseCoveredUIForm;
            private object mUserData;

            public OpenUIFormInfo()
            {
                mSerialId = 0;
                mUIGroup = null;
                mPauseCoveredUIForm = false;
                mUserData = null;
            }

            public int SerialId
            {
                get
                {
                    return mSerialId;
                }
            }

            public UIGroup UIGroup
            {
                get
                {
                    return mUIGroup;
                }
            }

            public bool PauseCoveredUIForm
            {
                get
                {
                    return mPauseCoveredUIForm;
                }
            }

            public object UserData
            {
                get
                {
                    return mUserData;
                }
            }

            public static OpenUIFormInfo Create(int serialId, UIGroup uiGroup, bool pauseCoveredUIForm, object userData)
            {
                OpenUIFormInfo openUIFormInfo = ReferencePool.Acquire<OpenUIFormInfo>();
                openUIFormInfo.mSerialId = serialId;
                openUIFormInfo.mUIGroup = uiGroup;
                openUIFormInfo.mPauseCoveredUIForm = pauseCoveredUIForm;
                openUIFormInfo.mUserData = userData;
                return openUIFormInfo;
            }

            public void Clear()
            {
                mSerialId = 0;
                mUIGroup = null;
                mPauseCoveredUIForm = false;
                mUserData = null;
            }
        }
    }
}
