//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.UI
{
    public interface IUIForm
    {
        int SerialId
        {
            get;
        }

        string UIFormAssetName
        {
            get;
        }

        object Handle
        {
            get;
        }

        IUIGroup UIGroup
        {
            get;
        }

        int DepthInUIGroup
        {
            get;
        }

        bool PauseCoveredUIForm
        {
            get;
        }

        void OnInit(int serialId, string uiFormAssetName, IUIGroup uiGroup, bool pauseCoveredUIForm, bool isNewInstance, object userData);

        void OnRecycle();

        void OnOpen(object userData);

        void OnClose(bool isShutdown, object userData);

        void OnPause();

        void OnResume();

        void OnCover();

        void OnReveal();

        void OnRefocus(object userData);

        void OnUpdate(float elapseSeconds, float realElapseSeconds);

        void OnDepthChanged(int uiGroupDepth, int depthInUIGroup);
    }
}
