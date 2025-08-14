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

namespace GameFramework.DataTable
{
    public interface IDataTableManager
    {
        int Count
        {
            get;
        }

        int CachedBytesSize
        {
            get;
        }

        void SetResourceManager(IResourceManager resourceManager);

        void SetDataProviderHelper(IDataProviderHelper<DataTableBase> dataProviderHelper);

        void SetDataTableHelper(IDataTableHelper dataTableHelper);

        void EnsureCachedBytesSize(int ensureSize);

        void FreeCachedBytes();

        bool HasDataTable<T>() where T : IDataRow;

        bool HasDataTable(Type dataRowType);

        bool HasDataTable<T>(string name) where T : IDataRow;

        bool HasDataTable(Type dataRowType, string name);

        IDataTable<T> GetDataTable<T>() where T : IDataRow;

        DataTableBase GetDataTable(Type dataRowType);

        IDataTable<T> GetDataTable<T>(string name) where T : IDataRow;

        DataTableBase GetDataTable(Type dataRowType, string name);

        DataTableBase[] GetAllDataTables();

        void GetAllDataTables(List<DataTableBase> results);

        IDataTable<T> CreateDataTable<T>() where T : class, IDataRow, new();

        DataTableBase CreateDataTable(Type dataRowType);

        IDataTable<T> CreateDataTable<T>(string name) where T : class, IDataRow, new();

        DataTableBase CreateDataTable(Type dataRowType, string name);

        bool DestroyDataTable<T>() where T : IDataRow;

        bool DestroyDataTable(Type dataRowType);

        bool DestroyDataTable<T>(string name) where T : IDataRow;

        bool DestroyDataTable(Type dataRowType, string name);

        bool DestroyDataTable<T>(IDataTable<T> dataTable) where T : IDataRow;

        bool DestroyDataTable(DataTableBase dataTable);
    }
}
