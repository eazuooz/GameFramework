//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFramework.DataTable
{
    internal sealed partial class DataTableManager : GameFrameworkModule, IDataTableManager
    {
        private sealed class DataTable<T> : DataTableBase, IDataTable<T> where T : class, IDataRow, new()
        {
            private readonly Dictionary<int, T> mDataSet;
            private T mMinIdDataRow;
            private T mMaxIdDataRow;

            public DataTable(string name)
                : base(name)
            {
                mDataSet = new Dictionary<int, T>();
                mMinIdDataRow = null;
                mMaxIdDataRow = null;
            }

            public override Type Type
            {
                get
                {
                    return typeof(T);
                }
            }

            public override int Count
            {
                get
                {
                    return mDataSet.Count;
                }
            }

            public T this[int id]
            {
                get
                {
                    return GetDataRow(id);
                }
            }

            public T MinIdDataRow
            {
                get
                {
                    return mMinIdDataRow;
                }
            }

            public T MaxIdDataRow
            {
                get
                {
                    return mMaxIdDataRow;
                }
            }

            public override bool HasDataRow(int id)
            {
                return mDataSet.ContainsKey(id);
            }

            public bool HasDataRow(Predicate<T> condition)
            {
                if (condition == null)
                {
                    throw new GameFrameworkException("Condition is invalid.");
                }

                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    if (condition(dataRow.Value))
                    {
                        return true;
                    }
                }

                return false;
            }

            public T GetDataRow(int id)
            {
                T dataRow = null;
                if (mDataSet.TryGetValue(id, out dataRow))
                {
                    return dataRow;
                }

                return null;
            }

            public T GetDataRow(Predicate<T> condition)
            {
                if (condition == null)
                {
                    throw new GameFrameworkException("Condition is invalid.");
                }

                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    if (condition(dataRow.Value))
                    {
                        return dataRow.Value;
                    }
                }

                return null;
            }

            public T[] GetDataRows(Predicate<T> condition)
            {
                if (condition == null)
                {
                    throw new GameFrameworkException("Condition is invalid.");
                }

                List<T> results = new List<T>();
                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    if (condition(dataRow.Value))
                    {
                        results.Add(dataRow.Value);
                    }
                }

                return results.ToArray();
            }

            public void GetDataRows(Predicate<T> condition, List<T> results)
            {
                if (condition == null)
                {
                    throw new GameFrameworkException("Condition is invalid.");
                }

                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    if (condition(dataRow.Value))
                    {
                        results.Add(dataRow.Value);
                    }
                }
            }

            public T[] GetDataRows(Comparison<T> comparison)
            {
                if (comparison == null)
                {
                    throw new GameFrameworkException("Comparison is invalid.");
                }

                List<T> results = new List<T>();
                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    results.Add(dataRow.Value);
                }

                results.Sort(comparison);
                return results.ToArray();
            }

            public void GetDataRows(Comparison<T> comparison, List<T> results)
            {
                if (comparison == null)
                {
                    throw new GameFrameworkException("Comparison is invalid.");
                }

                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    results.Add(dataRow.Value);
                }

                results.Sort(comparison);
            }

            public T[] GetDataRows(Predicate<T> condition, Comparison<T> comparison)
            {
                if (condition == null)
                {
                    throw new GameFrameworkException("Condition is invalid.");
                }

                if (comparison == null)
                {
                    throw new GameFrameworkException("Comparison is invalid.");
                }

                List<T> results = new List<T>();
                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    if (condition(dataRow.Value))
                    {
                        results.Add(dataRow.Value);
                    }
                }

                results.Sort(comparison);
                return results.ToArray();
            }

            public void GetDataRows(Predicate<T> condition, Comparison<T> comparison, List<T> results)
            {
                if (condition == null)
                {
                    throw new GameFrameworkException("Condition is invalid.");
                }

                if (comparison == null)
                {
                    throw new GameFrameworkException("Comparison is invalid.");
                }

                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    if (condition(dataRow.Value))
                    {
                        results.Add(dataRow.Value);
                    }
                }

                results.Sort(comparison);
            }

            public T[] GetAllDataRows()
            {
                int index = 0;
                T[] results = new T[mDataSet.Count];
                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    results[index++] = dataRow.Value;
                }

                return results;
            }

            public void GetAllDataRows(List<T> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (KeyValuePair<int, T> dataRow in mDataSet)
                {
                    results.Add(dataRow.Value);
                }
            }

            public override bool AddDataRow(string dataRowString, object userData)
            {
                try
                {
                    T dataRow = new T();
                    if (!dataRow.ParseDataRow(dataRowString, userData))
                    {
                        return false;
                    }

                    InternalAddDataRow(dataRow);
                    return true;
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Utility.Text.Format("Can not parse data row string for data table '{0}' with exception '{1}'.", new TypeNamePair(typeof(T), Name), exception), exception);
                }
            }

            public override bool AddDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
            {
                try
                {
                    T dataRow = new T();
                    if (!dataRow.ParseDataRow(dataRowBytes, startIndex, length, userData))
                    {
                        return false;
                    }

                    InternalAddDataRow(dataRow);
                    return true;
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(Utility.Text.Format("Can not parse data row bytes for data table '{0}' with exception '{1}'.", new TypeNamePair(typeof(T), Name), exception), exception);
                }
            }

            public override bool RemoveDataRow(int id)
            {
                if (!HasDataRow(id))
                {
                    return false;
                }

                if (!mDataSet.Remove(id))
                {
                    return false;
                }

                if (mMinIdDataRow != null && mMinIdDataRow.Id == id || mMaxIdDataRow != null && mMaxIdDataRow.Id == id)
                {
                    mMinIdDataRow = null;
                    mMaxIdDataRow = null;
                    foreach (KeyValuePair<int, T> dataRow in mDataSet)
                    {
                        if (mMinIdDataRow == null || mMinIdDataRow.Id > dataRow.Key)
                        {
                            mMinIdDataRow = dataRow.Value;
                        }

                        if (mMaxIdDataRow == null || mMaxIdDataRow.Id < dataRow.Key)
                        {
                            mMaxIdDataRow = dataRow.Value;
                        }
                    }
                }

                return true;
            }

            public override void RemoveAllDataRows()
            {
                mDataSet.Clear();
                mMinIdDataRow = null;
                mMaxIdDataRow = null;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return mDataSet.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return mDataSet.Values.GetEnumerator();
            }

            internal override void Shutdown()
            {
                mDataSet.Clear();
            }

            private void InternalAddDataRow(T dataRow)
            {
                if (mDataSet.ContainsKey(dataRow.Id))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Already exist '{0}' in data table '{1}'.", dataRow.Id, new TypeNamePair(typeof(T), Name)));
                }

                mDataSet.Add(dataRow.Id, dataRow);

                if (mMinIdDataRow == null || mMinIdDataRow.Id > dataRow.Id)
                {
                    mMinIdDataRow = dataRow;
                }

                if (mMaxIdDataRow == null || mMaxIdDataRow.Id < dataRow.Id)
                {
                    mMaxIdDataRow = dataRow;
                }
            }
        }
    }
}
