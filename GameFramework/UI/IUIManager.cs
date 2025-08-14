//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using System.Collections.Generic;

namespace GameFramework.UI
{
    public interface IUIManager
    {
        int UIGroupCount
        {
            get;
        }

        float InstanceAutoReleaseInterval
        {
            get;
            set;
        }

        int InstanceCapacity
        {
            get;
            set;
        }

        float InstanceExpireTime
        {
            get;
            set;
        }

        int InstancePriority
        {
            get;
            set;
        }

        event EventHandler<OpenUIFormSuccessEventArgs> OpenUIFormSuccess;

        event EventHandler<OpenUIFormFailureEventArgs> OpenUIFormFailure;

        event EventHandler<OpenUIFormUpdateEventArgs> OpenUIFormUpdate;

        event EventHandler<OpenUIFormDependencyAssetEventArgs> OpenUIFormDependencyAsset;

        event EventHandler<CloseUIFormCompleteEventArgs> CloseUIFormComplete;

        void SetObjectPoolManager(IObjectPoolManager objectPoolManager);

        void SetResourceManager(IResourceManager resourceManager);

        void SetUIFormHelper(IUIFormHelper uiFormHelper);

        bool HasUIGroup(string uiGroupName);

        IUIGroup GetUIGroup(string uiGroupName);

        IUIGroup[] GetAllUIGroups();

        void GetAllUIGroups(List<IUIGroup> results);

        bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper);

        bool AddUIGroup(string uiGroupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper);

        bool HasUIForm(int serialId);

        bool HasUIForm(string uiFormAssetName);

        IUIForm GetUIForm(int serialId);

        IUIForm GetUIForm(string uiFormAssetName);

        IUIForm[] GetUIForms(string uiFormAssetName);

        void GetUIForms(string uiFormAssetName, List<IUIForm> results);

        IUIForm[] GetAllLoadedUIForms();

        void GetAllLoadedUIForms(List<IUIForm> results);

        int[] GetAllLoadingUIFormSerialIds();

        void GetAllLoadingUIFormSerialIds(List<int> results);

        bool IsLoadingUIForm(int serialId);

        bool IsLoadingUIForm(string uiFormAssetName);

        bool IsValidUIForm(IUIForm uiForm);

        int OpenUIForm(string uiFormAssetName, string uiGroupName);

        int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority);

        int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm);

        int OpenUIForm(string uiFormAssetName, string uiGroupName, object userData);

        int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm);

        int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, object userData);

        int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData);

        int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData);

        void CloseUIForm(int serialId);

        void CloseUIForm(int serialId, object userData);

        void CloseUIForm(IUIForm uiForm);

        void CloseUIForm(IUIForm uiForm, object userData);

        void CloseAllLoadedUIForms();

        void CloseAllLoadedUIForms(object userData);

        void CloseAllLoadingUIForms();

        void RefocusUIForm(IUIForm uiForm);

        void RefocusUIForm(IUIForm uiForm, object userData);

        void SetUIFormInstanceLocked(object uiFormInstance, bool locked);

        void SetUIFormInstancePriority(object uiFormInstance, int priority);
    }
}
