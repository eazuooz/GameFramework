//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;

namespace GameFramework
{
    public interface IDataProvider<T>
    {
        event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess;

        event EventHandler<ReadDataFailureEventArgs> ReadDataFailure;

        event EventHandler<ReadDataUpdateEventArgs> ReadDataUpdate;

        event EventHandler<ReadDataDependencyAssetEventArgs> ReadDataDependencyAsset;

        void ReadData(string dataAssetName);

        void ReadData(string dataAssetName, int priority);

        void ReadData(string dataAssetName, object userData);

        void ReadData(string dataAssetName, int priority, object userData);

        bool ParseData(string dataString);

        bool ParseData(string dataString, object userData);

        bool ParseData(byte[] dataBytes);

        bool ParseData(byte[] dataBytes, object userData);

        bool ParseData(byte[] dataBytes, int startIndex, int length);

        bool ParseData(byte[] dataBytes, int startIndex, int length, object userData);
    }
}