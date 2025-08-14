//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.DataNode
{
    internal sealed partial class DataNodeManager : GameFrameworkModule, IDataNodeManager
    {
        private sealed class DataNode : IDataNode, IReference
        {
            private static readonly DataNode[] EmptyDataNodeArray = new DataNode[] { };

            private string mName;
            private Variable mData;
            private DataNode mParent;
            private List<DataNode> mChilds;

            public DataNode()
            {
                mName = null;
                mData = null;
                mParent = null;
                mChilds = null;
            }

            public static DataNode Create(string name, DataNode parent)
            {
                if (!IsValidName(name))
                {
                    throw new GameFrameworkException("Name of data node is invalid.");
                }

                DataNode node = ReferencePool.Acquire<DataNode>();
                node.mName = name;
                node.mParent = parent;
                return node;
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
                    return mParent == null ? mName : Utility.Text.Format("{0}{1}{2}", mParent.FullName, PathSplitSeparator[0], mName);
                }
            }

            public IDataNode Parent
            {
                get
                {
                    return mParent;
                }
            }

            public int ChildCount
            {
                get
                {
                    return mChilds != null ? mChilds.Count : 0;
                }
            }

            public T GetData<T>() where T : Variable
            {
                return (T)mData;
            }

            public Variable GetData()
            {
                return mData;
            }

            public void SetData<T>(T data) where T : Variable
            {
                SetData((Variable)data);
            }

            public void SetData(Variable data)
            {
                if (mData != null)
                {
                    ReferencePool.Release(mData);
                }

                mData = data;
            }

            public bool HasChild(int index)
            {
                return index >= 0 && index < ChildCount;
            }

            public bool HasChild(string name)
            {
                if (!IsValidName(name))
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                if (mChilds == null)
                {
                    return false;
                }

                foreach (DataNode child in mChilds)
                {
                    if (child.Name == name)
                    {
                        return true;
                    }
                }

                return false;
            }

            public IDataNode GetChild(int index)
            {
                return index >= 0 && index < ChildCount ? mChilds[index] : null;
            }

            public IDataNode GetChild(string name)
            {
                if (!IsValidName(name))
                {
                    throw new GameFrameworkException("Name is invalid.");
                }

                if (mChilds == null)
                {
                    return null;
                }

                foreach (DataNode child in mChilds)
                {
                    if (child.Name == name)
                    {
                        return child;
                    }
                }

                return null;
            }

            public IDataNode GetOrAddChild(string name)
            {
                DataNode node = (DataNode)GetChild(name);
                if (node != null)
                {
                    return node;
                }

                node = Create(name, this);

                if (mChilds == null)
                {
                    mChilds = new List<DataNode>();
                }

                mChilds.Add(node);

                return node;
            }

            public IDataNode[] GetAllChild()
            {
                if (mChilds == null)
                {
                    return EmptyDataNodeArray;
                }

                return mChilds.ToArray();
            }

            public void GetAllChild(List<IDataNode> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                if (mChilds == null)
                {
                    return;
                }

                foreach (DataNode child in mChilds)
                {
                    results.Add(child);
                }
            }

            public void RemoveChild(int index)
            {
                DataNode node = (DataNode)GetChild(index);
                if (node == null)
                {
                    return;
                }

                mChilds.Remove(node);
                ReferencePool.Release(node);
            }

            public void RemoveChild(string name)
            {
                DataNode node = (DataNode)GetChild(name);
                if (node == null)
                {
                    return;
                }

                mChilds.Remove(node);
                ReferencePool.Release(node);
            }

            public void Clear()
            {
                if (mData != null)
                {
                    ReferencePool.Release(mData);
                    mData = null;
                }

                if (mChilds != null)
                {
                    foreach (DataNode child in mChilds)
                    {
                        ReferencePool.Release(child);
                    }

                    mChilds.Clear();
                }
            }

            public override string ToString()
            {
                return Utility.Text.Format("{0}: {1}", FullName, ToDataString());
            }

            public string ToDataString()
            {
                if (mData == null)
                {
                    return "<Null>";
                }

                return Utility.Text.Format("[{0}] {1}", mData.Type.Name, mData);
            }

            private static bool IsValidName(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }

                foreach (string pathSplitSeparator in PathSplitSeparator)
                {
                    if (name.Contains(pathSplitSeparator))
                    {
                        return false;
                    }
                }

                return true;
            }

            void IReference.Clear()
            {
                mName = null;
                mParent = null;
                Clear();
            }
        }
    }
}
