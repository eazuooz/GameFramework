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
    public interface IUIGroup
    {
        string Name
        {
            get;
        }

        int Depth
        {
            get;
            set;
        }

        bool Pause
        {
            get;
            set;
        }

        int UIFormCount
        {
            get;
        }

        IUIForm CurrentUIForm
        {
            get;
        }

        IUIGroupHelper Helper
        {
            get;
        }

        bool HasUIForm(int serialId);

        bool HasUIForm(string uiFormAssetName);

        IUIForm GetUIForm(int serialId);

        IUIForm GetUIForm(string uiFormAssetName);

        IUIForm[] GetUIForms(string uiFormAssetName);

        void GetUIForms(string uiFormAssetName, List<IUIForm> results);

        IUIForm[] GetAllUIForms();

        void GetAllUIForms(List<IUIForm> results);
    }
}
