//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.Resource;
using System;

namespace GameFramework.DataTable
{
    public abstract class DataTableBase : IDataProvider<DataTableBase>
    {
        private readonly string mName;
        private readonly DataProvider<DataTableBase> mDataProvider;

        public DataTableBase()
            : this(null)
        {
        }

        public DataTableBase(string name)
        {
            mName = name ?? string.Empty;
            mDataProvider = new DataProvider<DataTableBase>(this);
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public string FullName
        {
            get
            {
                return new TypeNamePair(Type, mName).ToString();
            }
        }

        public abstract Type Type
        {
            get;
        }

        public abstract int Count
        {
            get;
        }

        public event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess
        {
            add
            {
                mDataProvider.ReadDataSuccess += value;
            }
            remove
            {
                mDataProvider.ReadDataSuccess -= value;
            }
        }

        public event EventHandler<ReadDataFailureEventArgs> ReadDataFailure
        {
            add
            {
                mDataProvider.ReadDataFailure += value;
            }
            remove
            {
                mDataProvider.ReadDataFailure -= value;
            }
        }

        public event EventHandler<ReadDataUpdateEventArgs> ReadDataUpdate
        {
            add
            {
                mDataProvider.ReadDataUpdate += value;
            }
            remove
            {
                mDataProvider.ReadDataUpdate -= value;
            }
        }

        public event EventHandler<ReadDataDependencyAssetEventArgs> ReadDataDependencyAsset
        {
            add
            {
                mDataProvider.ReadDataDependencyAsset += value;
            }
            remove
            {
                mDataProvider.ReadDataDependencyAsset -= value;
            }
        }

        public void ReadData(string dataTableAssetName)
        {
            mDataProvider.ReadData(dataTableAssetName);
        }

        public void ReadData(string dataTableAssetName, int priority)
        {
            mDataProvider.ReadData(dataTableAssetName, priority);
        }

        public void ReadData(string dataTableAssetName, object userData)
        {
            mDataProvider.ReadData(dataTableAssetName, userData);
        }

        public void ReadData(string dataTableAssetName, int priority, object userData)
        {
            mDataProvider.ReadData(dataTableAssetName, priority, userData);
        }

        public bool ParseData(string dataTableString)
        {
            return mDataProvider.ParseData(dataTableString);
        }

        public bool ParseData(string dataTableString, object userData)
        {
            return mDataProvider.ParseData(dataTableString, userData);
        }

        public bool ParseData(byte[] dataTableBytes)
        {
            return mDataProvider.ParseData(dataTableBytes);
        }

        public bool ParseData(byte[] dataTableBytes, object userData)
        {
            return mDataProvider.ParseData(dataTableBytes, userData);
        }

        public bool ParseData(byte[] dataTableBytes, int startIndex, int length)
        {
            return mDataProvider.ParseData(dataTableBytes, startIndex, length);
        }

        public bool ParseData(byte[] dataTableBytes, int startIndex, int length, object userData)
        {
            return mDataProvider.ParseData(dataTableBytes, startIndex, length, userData);
        }

        public abstract bool HasDataRow(int id);

        public abstract bool AddDataRow(string dataRowString, object userData);

        public abstract bool AddDataRow(byte[] dataRowBytes, int startIndex, int length, object userData);

        public abstract bool RemoveDataRow(int id);

        public abstract void RemoveAllDataRows();

        internal void SetResourceManager(IResourceManager resourceManager)
        {
            mDataProvider.SetResourceManager(resourceManager);
        }

        internal void SetDataProviderHelper(IDataProviderHelper<DataTableBase> dataProviderHelper)
        {
            mDataProvider.SetDataProviderHelper(dataProviderHelper);
        }

        internal abstract void Shutdown();
    }
}
