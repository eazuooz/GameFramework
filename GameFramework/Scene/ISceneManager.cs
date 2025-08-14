//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Resource;
using System;
using System.Collections.Generic;

namespace GameFramework.Scene
{
    public interface ISceneManager
    {
        event EventHandler<LoadSceneSuccessEventArgs> LoadSceneSuccess;

        event EventHandler<LoadSceneFailureEventArgs> LoadSceneFailure;

        event EventHandler<LoadSceneUpdateEventArgs> LoadSceneUpdate;

        event EventHandler<LoadSceneDependencyAssetEventArgs> LoadSceneDependencyAsset;

        event EventHandler<UnloadSceneSuccessEventArgs> UnloadSceneSuccess;

        event EventHandler<UnloadSceneFailureEventArgs> UnloadSceneFailure;

        void SetResourceManager(IResourceManager resourceManager);

        bool SceneIsLoaded(string sceneAssetName);

        string[] GetLoadedSceneAssetNames();

        void GetLoadedSceneAssetNames(List<string> results);

        bool SceneIsLoading(string sceneAssetName);

        string[] GetLoadingSceneAssetNames();

        void GetLoadingSceneAssetNames(List<string> results);

        bool SceneIsUnloading(string sceneAssetName);

        string[] GetUnloadingSceneAssetNames();

        void GetUnloadingSceneAssetNames(List<string> results);

        bool HasScene(string sceneAssetName);

        void LoadScene(string sceneAssetName);

        void LoadScene(string sceneAssetName, int priority);

        void LoadScene(string sceneAssetName, object userData);

        void LoadScene(string sceneAssetName, int priority, object userData);

        void UnloadScene(string sceneAssetName);

        void UnloadScene(string sceneAssetName, object userData);
    }
}
