//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    public sealed class ReadDataDependencyAssetEventArgs : GameFrameworkEventArgs
    {
        public ReadDataDependencyAssetEventArgs()
        {
            DataAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }

        public static ReadDataDependencyAssetEventArgs Create(string dataAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            ReadDataDependencyAssetEventArgs loadDataDependencyAssetEventArgs = ReferencePool.Acquire<ReadDataDependencyAssetEventArgs>();
            loadDataDependencyAssetEventArgs.DataAssetName = dataAssetName;
            loadDataDependencyAssetEventArgs.DependencyAssetName = dependencyAssetName;
            loadDataDependencyAssetEventArgs.LoadedCount = loadedCount;
            loadDataDependencyAssetEventArgs.TotalCount = totalCount;
            loadDataDependencyAssetEventArgs.UserData = userData;
            return loadDataDependencyAssetEventArgs;
        }

        public override void Clear()
        {
            DataAssetName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }

        public string DataAssetName
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
    }
}