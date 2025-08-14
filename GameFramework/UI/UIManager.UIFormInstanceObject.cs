//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.ObjectPool;

namespace GameFramework.UI
{
    internal sealed partial class UIManager : GameFrameworkModule, IUIManager
    {
        private sealed class UIFormInstanceObject : ObjectBase
        {
            private object mUIFormAsset;
            private IUIFormHelper mUIFormHelper;

            public UIFormInstanceObject()
            {
                mUIFormAsset = null;
                mUIFormHelper = null;
            }

            public static UIFormInstanceObject Create(string name, object uiFormAsset, object uiFormInstance, IUIFormHelper uiFormHelper)
            {
                if (uiFormAsset == null)
                {
                    throw new GameFrameworkException("UI form asset is invalid.");
                }

                if (uiFormHelper == null)
                {
                    throw new GameFrameworkException("UI form helper is invalid.");
                }

                UIFormInstanceObject uiFormInstanceObject = ReferencePool.Acquire<UIFormInstanceObject>();
                uiFormInstanceObject.Initialize(name, uiFormInstance);
                uiFormInstanceObject.mUIFormAsset = uiFormAsset;
                uiFormInstanceObject.mUIFormHelper = uiFormHelper;
                return uiFormInstanceObject;
            }

            public override void Clear()
            {
                base.Clear();
                mUIFormAsset = null;
                mUIFormHelper = null;
            }

            protected internal override void Release(bool isShutdown)
            {
                mUIFormHelper.ReleaseUIForm(mUIFormAsset, Target);
            }
        }
    }
}
