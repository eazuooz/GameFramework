//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Resource;
using System;

namespace GameFramework
{
    internal sealed class DataProvider<T> : IDataProvider<T>
    {
        public DataProvider(T owner)
        {
            mOwner = owner;
            mLoadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetOrBinaryFailureCallback, LoadAssetUpdateCallback, LoadAssetDependencyAssetCallback);
            mLoadBinaryCallbacks = new LoadBinaryCallbacks(LoadBinarySuccessCallback, LoadAssetOrBinaryFailureCallback);
            mResourceManager = null;
            mDataProviderHelper = null;
            mReadDataSuccessEventHandler = null;
            mReadDataFailureEventHandler = null;
            mReadDataUpdateEventHandler = null;
            mReadDataDependencyAssetEventHandler = null;
        }

        public static int CachedBytesSize
        {
            get
            {
                return CachedBytes != null ? CachedBytes.Length : 0;
            }
        }

        public event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess
        {
            add
            {
                mReadDataSuccessEventHandler += value;
            }
            remove
            {
                mReadDataSuccessEventHandler -= value;
            }
        }

        public event EventHandler<ReadDataFailureEventArgs> ReadDataFailure
        {
            add
            {
                mReadDataFailureEventHandler += value;
            }
            remove
            {
                mReadDataFailureEventHandler -= value;
            }
        }

        public event EventHandler<ReadDataUpdateEventArgs> ReadDataUpdate
        {
            add
            {
                mReadDataUpdateEventHandler += value;
            }
            remove
            {
                mReadDataUpdateEventHandler -= value;
            }
        }

        public event EventHandler<ReadDataDependencyAssetEventArgs> ReadDataDependencyAsset
        {
            add
            {
                mReadDataDependencyAssetEventHandler += value;
            }
            remove
            {
                mReadDataDependencyAssetEventHandler -= value;
            }
        }

        public static void EnsureCachedBytesSize(int ensureSize)
        {
            if (ensureSize < 0)
            {
                throw new GameFrameworkException("Ensure size is invalid.");
            }

            if (CachedBytes == null || CachedBytes.Length < ensureSize)
            {
                FreeCachedBytes();
                int size = (ensureSize - 1 + BlockSize) / BlockSize * BlockSize;
                CachedBytes = new byte[size];
            }
        }

        public static void FreeCachedBytes()
        {
            CachedBytes = null;
        }

        public void ReadData(string dataAssetName)
        {
            ReadData(dataAssetName, Constant.DefaultPriority, null);
        }

        public void ReadData(string dataAssetName, int priority)
        {
            ReadData(dataAssetName, priority, null);
        }

        public void ReadData(string dataAssetName, object userData)
        {
            ReadData(dataAssetName, Constant.DefaultPriority, userData);
        }

        public void ReadData(string dataAssetName, int priority, object userData)
        {
            if (mResourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (mDataProviderHelper == null)
            {
                throw new GameFrameworkException("You must set data provider helper first.");
            }

            HasAssetResult result = mResourceManager.HasAsset(dataAssetName);
            switch (result)
            {
                case HasAssetResult.AssetOnDisk:
                case HasAssetResult.AssetOnFileSystem:
                    mResourceManager.LoadAsset(dataAssetName, priority, mLoadAssetCallbacks, userData);
                    break;

                case HasAssetResult.BinaryOnDisk:
                    mResourceManager.LoadBinary(dataAssetName, mLoadBinaryCallbacks, userData);
                    break;

                case HasAssetResult.BinaryOnFileSystem:
                    int dataLength = mResourceManager.GetBinaryLength(dataAssetName);
                    EnsureCachedBytesSize(dataLength);
                    if (dataLength != mResourceManager.LoadBinaryFromFileSystem(dataAssetName, CachedBytes))
                    {
                        throw new GameFrameworkException(Utility.Text.Format("Load binary '{0}' from file system with internal error.", dataAssetName));
                    }

                    try
                    {
                        if (!mDataProviderHelper.ReadData(mOwner, dataAssetName, CachedBytes, 0, dataLength, userData))
                        {
                            throw new GameFrameworkException(Utility.Text.Format("Load data failure in data provider helper, data asset name '{0}'.", dataAssetName));
                        }

                        if (mReadDataSuccessEventHandler != null)
                        {
                            ReadDataSuccessEventArgs loadDataSuccessEventArgs = ReadDataSuccessEventArgs.Create(dataAssetName, 0f, userData);
                            mReadDataSuccessEventHandler(this, loadDataSuccessEventArgs);
                            ReferencePool.Release(loadDataSuccessEventArgs);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (mReadDataFailureEventHandler != null)
                        {
                            ReadDataFailureEventArgs loadDataFailureEventArgs = ReadDataFailureEventArgs.Create(dataAssetName, exception.ToString(), userData);
                            mReadDataFailureEventHandler(this, loadDataFailureEventArgs);
                            ReferencePool.Release(loadDataFailureEventArgs);
                            return;
                        }

                        throw;
                    }

                    break;

                default:
                    throw new GameFrameworkException(Utility.Text.Format("Data asset '{0}' is '{1}'.", dataAssetName, result));
            }
        }

        public bool ParseData(string dataString)
        {
            return ParseData(dataString, null);
        }

        public bool ParseData(string dataString, object userData)
        {
            if (mDataProviderHelper == null)
            {
                throw new GameFrameworkException("You must set data helper first.");
            }

            if (dataString == null)
            {
                throw new GameFrameworkException("Data string is invalid.");
            }

            try
            {
                return mDataProviderHelper.ParseData(mOwner, dataString, userData);
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException)
                {
                    throw;
                }

                throw new GameFrameworkException(Utility.Text.Format("Can not parse data string with exception '{0}'.", exception), exception);
            }
        }

        public bool ParseData(byte[] dataBytes)
        {
            if (dataBytes == null)
            {
                throw new GameFrameworkException("Data bytes is invalid.");
            }

            return ParseData(dataBytes, 0, dataBytes.Length, null);
        }

        public bool ParseData(byte[] dataBytes, object userData)
        {
            if (dataBytes == null)
            {
                throw new GameFrameworkException("Data bytes is invalid.");
            }

            return ParseData(dataBytes, 0, dataBytes.Length, userData);
        }

        public bool ParseData(byte[] dataBytes, int startIndex, int length)
        {
            return ParseData(dataBytes, startIndex, length, null);
        }

        public bool ParseData(byte[] dataBytes, int startIndex, int length, object userData)
        {
            if (mDataProviderHelper == null)
            {
                throw new GameFrameworkException("You must set data helper first.");
            }

            if (dataBytes == null)
            {
                throw new GameFrameworkException("Data bytes is invalid.");
            }

            if (startIndex < 0 || length < 0 || startIndex + length > dataBytes.Length)
            {
                throw new GameFrameworkException("Start index or length is invalid.");
            }

            try
            {
                return mDataProviderHelper.ParseData(mOwner, dataBytes, startIndex, length, userData);
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException)
                {
                    throw;
                }

                throw new GameFrameworkException(Utility.Text.Format("Can not parse data bytes with exception '{0}'.", exception), exception);
            }
        }

        internal void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            mResourceManager = resourceManager;
        }

        internal void SetDataProviderHelper(IDataProviderHelper<T> dataProviderHelper)
        {
            if (dataProviderHelper == null)
            {
                throw new GameFrameworkException("Data provider helper is invalid.");
            }

            mDataProviderHelper = dataProviderHelper;
        }

        private void LoadAssetSuccessCallback(string dataAssetName, object dataAsset, float duration, object userData)
        {
            try
            {
                if (!mDataProviderHelper.ReadData(mOwner, dataAssetName, dataAsset, userData))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Load data failure in data provider helper, data asset name '{0}'.", dataAssetName));
                }

                if (mReadDataSuccessEventHandler != null)
                {
                    ReadDataSuccessEventArgs loadDataSuccessEventArgs = ReadDataSuccessEventArgs.Create(dataAssetName, duration, userData);
                    mReadDataSuccessEventHandler(this, loadDataSuccessEventArgs);
                    ReferencePool.Release(loadDataSuccessEventArgs);
                }
            }
            catch (Exception exception)
            {
                if (mReadDataFailureEventHandler != null)
                {
                    ReadDataFailureEventArgs loadDataFailureEventArgs = ReadDataFailureEventArgs.Create(dataAssetName, exception.ToString(), userData);
                    mReadDataFailureEventHandler(this, loadDataFailureEventArgs);
                    ReferencePool.Release(loadDataFailureEventArgs);
                    return;
                }

                throw;
            }
            finally
            {
                mDataProviderHelper.ReleaseDataAsset(mOwner, dataAsset);
            }
        }

        private void LoadAssetOrBinaryFailureCallback(string dataAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            string appendErrorMessage = Utility.Text.Format("Load data failure, data asset name '{0}', status '{1}', error message '{2}'.", dataAssetName, status, errorMessage);
            if (mReadDataFailureEventHandler != null)
            {
                ReadDataFailureEventArgs loadDataFailureEventArgs = ReadDataFailureEventArgs.Create(dataAssetName, appendErrorMessage, userData);
                mReadDataFailureEventHandler(this, loadDataFailureEventArgs);
                ReferencePool.Release(loadDataFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }

        private void LoadAssetUpdateCallback(string dataAssetName, float progress, object userData)
        {
            if (mReadDataUpdateEventHandler != null)
            {
                ReadDataUpdateEventArgs loadDataUpdateEventArgs = ReadDataUpdateEventArgs.Create(dataAssetName, progress, userData);
                mReadDataUpdateEventHandler(this, loadDataUpdateEventArgs);
                ReferencePool.Release(loadDataUpdateEventArgs);
            }
        }

        private void LoadAssetDependencyAssetCallback(string dataAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            if (mReadDataDependencyAssetEventHandler != null)
            {
                ReadDataDependencyAssetEventArgs loadDataDependencyAssetEventArgs = ReadDataDependencyAssetEventArgs.Create(dataAssetName, dependencyAssetName, loadedCount, totalCount, userData);
                mReadDataDependencyAssetEventHandler(this, loadDataDependencyAssetEventArgs);
                ReferencePool.Release(loadDataDependencyAssetEventArgs);
            }
        }

        private void LoadBinarySuccessCallback(string dataAssetName, byte[] dataBytes, float duration, object userData)
        {
            try
            {
                if (!mDataProviderHelper.ReadData(mOwner, dataAssetName, dataBytes, 0, dataBytes.Length, userData))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Load data failure in data provider helper, data asset name '{0}'.", dataAssetName));
                }

                if (mReadDataSuccessEventHandler != null)
                {
                    ReadDataSuccessEventArgs loadDataSuccessEventArgs = ReadDataSuccessEventArgs.Create(dataAssetName, duration, userData);
                    mReadDataSuccessEventHandler(this, loadDataSuccessEventArgs);
                    ReferencePool.Release(loadDataSuccessEventArgs);
                }
            }
            catch (Exception exception)
            {
                if (mReadDataFailureEventHandler != null)
                {
                    ReadDataFailureEventArgs loadDataFailureEventArgs = ReadDataFailureEventArgs.Create(dataAssetName, exception.ToString(), userData);
                    mReadDataFailureEventHandler(this, loadDataFailureEventArgs);
                    ReferencePool.Release(loadDataFailureEventArgs);
                    return;
                }

                throw;
            }
        }

        private const int BlockSize = 1024 * 4;
        private static byte[] CachedBytes = null;

        private readonly T mOwner;
        private readonly LoadAssetCallbacks mLoadAssetCallbacks;
        private readonly LoadBinaryCallbacks mLoadBinaryCallbacks;
        private IResourceManager mResourceManager;
        private IDataProviderHelper<T> mDataProviderHelper;
        private EventHandler<ReadDataSuccessEventArgs> mReadDataSuccessEventHandler;
        private EventHandler<ReadDataFailureEventArgs> mReadDataFailureEventHandler;
        private EventHandler<ReadDataUpdateEventArgs> mReadDataUpdateEventHandler;
        private EventHandler<ReadDataDependencyAssetEventArgs> mReadDataDependencyAssetEventHandler;
    }
}
