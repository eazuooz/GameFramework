//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Resource
{
    public interface IResourceHelper
    {
        void LoadBytes(string fileUri, LoadBytesCallbacks loadBytesCallbacks, object userData);

        void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData);

        void Release(object objectToRelease);
    }
}
