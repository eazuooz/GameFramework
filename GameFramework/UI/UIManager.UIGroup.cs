//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.UI
{
    internal sealed partial class UIManager : GameFrameworkModule, IUIManager
    {
        private sealed partial class UIGroup : IUIGroup
        {
            private readonly string mName;
            private int mDepth;
            private bool mPause;
            private readonly IUIGroupHelper mUIGroupHelper;
            private readonly GameFrameworkLinkedList<UIFormInfo> mUIFormInfos;
            private LinkedListNode<UIFormInfo> mCachedNode;

            public UIGroup(string name, int depth, IUIGroupHelper uiGroupHelper)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("UI group name is invalid.");
                }

                if (uiGroupHelper == null)
                {
                    throw new GameFrameworkException("UI group helper is invalid.");
                }

                mName = name;
                mPause = false;
                mUIGroupHelper = uiGroupHelper;
                mUIFormInfos = new GameFrameworkLinkedList<UIFormInfo>();
                mCachedNode = null;
                Depth = depth;
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public int Depth
            {
                get
                {
                    return mDepth;
                }
                set
                {
                    if (mDepth == value)
                    {
                        return;
                    }

                    mDepth = value;
                    mUIGroupHelper.SetDepth(mDepth);
                    Refresh();
                }
            }

            public bool Pause
            {
                get
                {
                    return mPause;
                }
                set
                {
                    if (mPause == value)
                    {
                        return;
                    }

                    mPause = value;
                    Refresh();
                }
            }

            public int UIFormCount
            {
                get
                {
                    return mUIFormInfos.Count;
                }
            }

            public IUIForm CurrentUIForm
            {
                get
                {
                    return mUIFormInfos.First != null ? mUIFormInfos.First.Value.UIForm : null;
                }
            }

            public IUIGroupHelper Helper
            {
                get
                {
                    return mUIGroupHelper;
                }
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                LinkedListNode<UIFormInfo> current = mUIFormInfos.First;
                while (current != null)
                {
                    if (current.Value.Paused)
                    {
                        break;
                    }

                    mCachedNode = current.Next;
                    current.Value.UIForm.OnUpdate(elapseSeconds, realElapseSeconds);
                    current = mCachedNode;
                    mCachedNode = null;
                }
            }

            public bool HasUIForm(int serialId)
            {
                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    if (uiFormInfo.UIForm.SerialId == serialId)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool HasUIForm(string uiFormAssetName)
            {
                if (string.IsNullOrEmpty(uiFormAssetName))
                {
                    throw new GameFrameworkException("UI form asset name is invalid.");
                }

                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                    {
                        return true;
                    }
                }

                return false;
            }

            public IUIForm GetUIForm(int serialId)
            {
                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    if (uiFormInfo.UIForm.SerialId == serialId)
                    {
                        return uiFormInfo.UIForm;
                    }
                }

                return null;
            }

            public IUIForm GetUIForm(string uiFormAssetName)
            {
                if (string.IsNullOrEmpty(uiFormAssetName))
                {
                    throw new GameFrameworkException("UI form asset name is invalid.");
                }

                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                    {
                        return uiFormInfo.UIForm;
                    }
                }

                return null;
            }

            public IUIForm[] GetUIForms(string uiFormAssetName)
            {
                if (string.IsNullOrEmpty(uiFormAssetName))
                {
                    throw new GameFrameworkException("UI form asset name is invalid.");
                }

                List<IUIForm> results = new List<IUIForm>();
                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                    {
                        results.Add(uiFormInfo.UIForm);
                    }
                }

                return results.ToArray();
            }

            public void GetUIForms(string uiFormAssetName, List<IUIForm> results)
            {
                if (string.IsNullOrEmpty(uiFormAssetName))
                {
                    throw new GameFrameworkException("UI form asset name is invalid.");
                }

                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                    {
                        results.Add(uiFormInfo.UIForm);
                    }
                }
            }

            public IUIForm[] GetAllUIForms()
            {
                List<IUIForm> results = new List<IUIForm>();
                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    results.Add(uiFormInfo.UIForm);
                }

                return results.ToArray();
            }

            public void GetAllUIForms(List<IUIForm> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    results.Add(uiFormInfo.UIForm);
                }
            }

            public void AddUIForm(IUIForm uiForm)
            {
                mUIFormInfos.AddFirst(UIFormInfo.Create(uiForm));
            }

            public void RemoveUIForm(IUIForm uiForm)
            {
                UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);
                if (uiFormInfo == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not find UI form info for serial id '{0}', UI form asset name is '{1}'.", uiForm.SerialId, uiForm.UIFormAssetName));
                }

                if (!uiFormInfo.Covered)
                {
                    uiFormInfo.Covered = true;
                    uiForm.OnCover();
                }

                if (!uiFormInfo.Paused)
                {
                    uiFormInfo.Paused = true;
                    uiForm.OnPause();
                }

                if (mCachedNode != null && mCachedNode.Value.UIForm == uiForm)
                {
                    mCachedNode = mCachedNode.Next;
                }

                if (!mUIFormInfos.Remove(uiFormInfo))
                {
                    throw new GameFrameworkException(Utility.Text.Format("UI group '{0}' not exists specified UI form '[{1}]{2}'.", mName, uiForm.SerialId, uiForm.UIFormAssetName));
                }

                ReferencePool.Release(uiFormInfo);
            }

            public void RefocusUIForm(IUIForm uiForm, object userData)
            {
                UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);
                if (uiFormInfo == null)
                {
                    throw new GameFrameworkException("Can not find UI form info.");
                }

                mUIFormInfos.Remove(uiFormInfo);
                mUIFormInfos.AddFirst(uiFormInfo);
            }

            public void Refresh()
            {
                LinkedListNode<UIFormInfo> current = mUIFormInfos.First;
                bool pause = mPause;
                bool cover = false;
                int depth = UIFormCount;
                while (current != null && current.Value != null)
                {
                    LinkedListNode<UIFormInfo> next = current.Next;
                    current.Value.UIForm.OnDepthChanged(Depth, depth--);
                    if (current.Value == null)
                    {
                        return;
                    }

                    if (pause)
                    {
                        if (!current.Value.Covered)
                        {
                            current.Value.Covered = true;
                            current.Value.UIForm.OnCover();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }

                        if (!current.Value.Paused)
                        {
                            current.Value.Paused = true;
                            current.Value.UIForm.OnPause();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (current.Value.Paused)
                        {
                            current.Value.Paused = false;
                            current.Value.UIForm.OnResume();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }

                        if (current.Value.UIForm.PauseCoveredUIForm)
                        {
                            pause = true;
                        }

                        if (cover)
                        {
                            if (!current.Value.Covered)
                            {
                                current.Value.Covered = true;
                                current.Value.UIForm.OnCover();
                                if (current.Value == null)
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (current.Value.Covered)
                            {
                                current.Value.Covered = false;
                                current.Value.UIForm.OnReveal();
                                if (current.Value == null)
                                {
                                    return;
                                }
                            }

                            cover = true;
                        }
                    }

                    current = next;
                }
            }

            internal void InternalGetUIForms(string uiFormAssetName, List<IUIForm> results)
            {
                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                    {
                        results.Add(uiFormInfo.UIForm);
                    }
                }
            }

            internal void InternalGetAllUIForms(List<IUIForm> results)
            {
                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    results.Add(uiFormInfo.UIForm);
                }
            }

            private UIFormInfo GetUIFormInfo(IUIForm uiForm)
            {
                if (uiForm == null)
                {
                    throw new GameFrameworkException("UI form is invalid.");
                }

                foreach (UIFormInfo uiFormInfo in mUIFormInfos)
                {
                    if (uiFormInfo.UIForm == uiForm)
                    {
                        return uiFormInfo;
                    }
                }

                return null;
            }
        }
    }
}
