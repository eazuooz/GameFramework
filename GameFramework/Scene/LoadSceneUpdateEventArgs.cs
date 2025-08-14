//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Scene
{
    public sealed class LoadSceneUpdateEventArgs : GameFrameworkEventArgs
    {
        public LoadSceneUpdateEventArgs()
        {
            SceneAssetName = null;
            Progress = 0f;
            UserData = null;
        }

        public string SceneAssetName
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

        public static LoadSceneUpdateEventArgs Create(string sceneAssetName, float progress, object userData)
        {
            LoadSceneUpdateEventArgs loadSceneUpdateEventArgs = ReferencePool.Acquire<LoadSceneUpdateEventArgs>();
            loadSceneUpdateEventArgs.SceneAssetName = sceneAssetName;
            loadSceneUpdateEventArgs.Progress = progress;
            loadSceneUpdateEventArgs.UserData = userData;
            return loadSceneUpdateEventArgs;
        }

        public override void Clear()
        {
            SceneAssetName = null;
            Progress = 0f;
            UserData = null;
        }
    }
}
