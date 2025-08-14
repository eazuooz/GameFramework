//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.DataNode
{
    public interface IDataNodeManager
    {
        IDataNode Root
        {
            get;
        }

        T GetData<T>(string path) where T : Variable;

        Variable GetData(string path);

        T GetData<T>(string path, IDataNode node) where T : Variable;

        Variable GetData(string path, IDataNode node);

        void SetData<T>(string path, T data) where T : Variable;

        void SetData(string path, Variable data);

        void SetData<T>(string path, T data, IDataNode node) where T : Variable;

        void SetData(string path, Variable data, IDataNode node);

        IDataNode GetNode(string path);

        IDataNode GetNode(string path, IDataNode node);

        IDataNode GetOrAddNode(string path);

        IDataNode GetOrAddNode(string path, IDataNode node);

        void RemoveNode(string path);

        void RemoveNode(string path, IDataNode node);

        void Clear();
    }
}
