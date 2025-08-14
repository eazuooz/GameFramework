//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework
{
    internal interface ITaskAgent<T> where T : TaskBase
    {
        T Task
        {
            get;
        }

        void Initialize();

        void Update(float elapseSeconds, float realElapseSeconds);

        void Shutdown();

        StartTaskStatus Start(T task);

        void Reset();
    }
}