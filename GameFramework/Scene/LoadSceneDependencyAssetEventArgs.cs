//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Scene
{
    public sealed class LoadSceneDependencyAssetEventArgs : GameFrameworkEventArgs
    {
        public LoadSceneDependencyAssetEventArgs()
        {
            SceneAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }

        public string SceneAssetName
        {
            get;
            private set;
        }

        public string DependencyAssetName
        {
            get;
            private set;
        }

        public int LoadedCount
        {
            get;
            private set;
        }

        public int TotalCount
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static LoadSceneDependencyAssetEventArgs Create(string sceneAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            LoadSceneDependencyAssetEventArgs loadSceneDependencyAssetEventArgs = ReferencePool.Acquire<LoadSceneDependencyAssetEventArgs>();
            loadSceneDependencyAssetEventArgs.SceneAssetName = sceneAssetName;
            loadSceneDependencyAssetEventArgs.DependencyAssetName = dependencyAssetName;
            loadSceneDependencyAssetEventArgs.LoadedCount = loadedCount;
            loadSceneDependencyAssetEventArgs.TotalCount = totalCount;
            loadSceneDependencyAssetEventArgs.UserData = userData;
            return loadSceneDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            SceneAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
}
