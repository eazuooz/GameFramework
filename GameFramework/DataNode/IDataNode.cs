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
    public interface IDataNode
    {
        string Name
        {
            get;
        }

        string FullName
        {
            get;
        }

        IDataNode Parent
        {
            get;
        }

        int ChildCount
        {
            get;
        }

        T GetData<T>() where T : Variable;

        Variable GetData();

        void SetData<T>(T data) where T : Variable;

        void SetData(Variable data);

        bool HasChild(int index);

        bool HasChild(string name);

        IDataNode GetChild(int index);

        IDataNode GetChild(string name);

        IDataNode GetOrAddChild(string name);

        IDataNode[] GetAllChild();

        void GetAllChild(List<IDataNode> results);

        void RemoveChild(int index);

        void RemoveChild(string name);

        void Clear();

        string ToString();

        string ToDataString();
    }
}
