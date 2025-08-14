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
        private sealed partial class UIGroup : IUIGroup
        {
            private sealed class UIFormInfo : IReference
            {
                private IUIForm mUIForm;
                private bool mPaused;
                private bool mCovered;

                public UIFormInfo()
                {
                    mUIForm = null;
                    mPaused = false;
                    mCovered = false;
                }

                public IUIForm UIForm
                {
                    get
                    {
                        return mUIForm;
                    }
                }

                public bool Paused
                {
                    get
                    {
                        return mPaused;
                    }
                    set
                    {
                        mPaused = value;
                    }
                }

                public bool Covered
                {
                    get
                    {
                        return mCovered;
                    }
                    set
                    {
                        mCovered = value;
                    }
                }

                public static UIFormInfo Create(IUIForm uiForm)
                {
                    if (uiForm == null)
                    {
                        throw new GameFrameworkException("UI form is invalid.");
                    }

                    UIFormInfo uiFormInfo = ReferencePool.Acquire<UIFormInfo>();
                    uiFormInfo.mUIForm = uiForm;
                    uiFormInfo.mPaused = true;
                    uiFormInfo.mCovered = true;
                    return uiFormInfo;
                }

                public void Clear()
                {
                    mUIForm = null;
                    mPaused = false;
                    mCovered = false;
                }
            }
        }
    }
}
