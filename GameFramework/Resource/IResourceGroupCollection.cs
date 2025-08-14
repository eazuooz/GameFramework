//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.Resource
{
    public interface IResourceGroupCollection
    {
        bool Ready
        {
            get;
        }

        int TotalCount
        {
            get;
        }

        int ReadyCount
        {
            get;
        }

        long TotalLength
        {
            get;
        }

        long TotalCompressedLength
        {
            get;
        }

        long ReadyLength
        {
            get;
        }

        long ReadyCompressedLength
        {
            get;
        }

        float Progress
        {
            get;
        }

        IResourceGroup[] GetResourceGroups();

        string[] GetResourceNames();

        void GetResourceNames(List<string> results);
    }
}
