//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework
{
    internal sealed class TaskPool<T> where T : TaskBase
    {
        public TaskPool()
        {
            mFreeAgents = new Stack<ITaskAgent<T>>();
            mWorkingAgents = new GameFrameworkLinkedList<ITaskAgent<T>>();
            mWaitingTasks = new GameFrameworkLinkedList<T>();
            mPaused = false;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (mPaused)
            {
                return;
            }

            ProcessRunningTasks(elapseSeconds, realElapseSeconds);
            ProcessWaitingTasks(elapseSeconds, realElapseSeconds);
        }

        public void Shutdown()
        {
            RemoveAllTasks();

            while (FreeAgentCount > 0)
            {
                mFreeAgents.Pop().Shutdown();
            }
        }

        public void AddAgent(ITaskAgent<T> agent)
        {
            if (agent == null)
            {
                throw new GameFrameworkException("Task agent is invalid.");
            }

            agent.Initialize();
            mFreeAgents.Push(agent);
        }

        public TaskInfo GetTaskInfo(int serialId)
        {
            foreach (ITaskAgent<T> workingAgent in mWorkingAgents)
            {
                T workingTask = workingAgent.Task;
                if (workingTask.SerialId == serialId)
                {
                    return new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description);
                }
            }

            foreach (T waitingTask in mWaitingTasks)
            {
                if (waitingTask.SerialId == serialId)
                {
                    return new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description);
                }
            }

            return default(TaskInfo);
        }

        public TaskInfo[] GetTaskInfos(string tag)
        {
            List<TaskInfo> results = new List<TaskInfo>();
            GetTaskInfos(tag, results);
            return results.ToArray();
        }

        public void GetTaskInfos(string tag, List<TaskInfo> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (ITaskAgent<T> workingAgent in mWorkingAgents)
            {
                T workingTask = workingAgent.Task;
                if (workingTask.Tag == tag)
                {
                    results.Add(new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description));
                }
            }

            foreach (T waitingTask in mWaitingTasks)
            {
                if (waitingTask.Tag == tag)
                {
                    results.Add(new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description));
                }
            }
        }

        public TaskInfo[] GetAllTaskInfos()
        {
            int index = 0;
            TaskInfo[] results = new TaskInfo[mWorkingAgents.Count + mWaitingTasks.Count];
            foreach (ITaskAgent<T> workingAgent in mWorkingAgents)
            {
                T workingTask = workingAgent.Task;
                results[index++] = new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description);
            }

            foreach (T waitingTask in mWaitingTasks)
            {
                results[index++] = new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description);
            }

            return results;
        }

        public void GetAllTaskInfos(List<TaskInfo> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (ITaskAgent<T> workingAgent in mWorkingAgents)
            {
                T workingTask = workingAgent.Task;
                results.Add(new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description));
            }

            foreach (T waitingTask in mWaitingTasks)
            {
                results.Add(new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description));
            }
        }

        public void AddTask(T task)
        {
            LinkedListNode<T> current = mWaitingTasks.Last;
            while (current != null)
            {
                if (task.Priority <= current.Value.Priority)
                {
                    break;
                }

                current = current.Previous;
            }

            if (current != null)
            {
                mWaitingTasks.AddAfter(current, task);
            }
            else
            {
                mWaitingTasks.AddFirst(task);
            }
        }

        public bool RemoveTask(int serialId)
        {
            foreach (T task in mWaitingTasks)
            {
                if (task.SerialId == serialId)
                {
                    mWaitingTasks.Remove(task);
                    ReferencePool.Release(task);
                    return true;
                }
            }

            LinkedListNode<ITaskAgent<T>> currentWorkingAgent = mWorkingAgents.First;
            while (currentWorkingAgent != null)
            {
                LinkedListNode<ITaskAgent<T>> next = currentWorkingAgent.Next;
                ITaskAgent<T> workingAgent = currentWorkingAgent.Value;
                T task = workingAgent.Task;
                if (task.SerialId == serialId)
                {
                    workingAgent.Reset();
                    mFreeAgents.Push(workingAgent);
                    mWorkingAgents.Remove(currentWorkingAgent);
                    ReferencePool.Release(task);
                    return true;
                }

                currentWorkingAgent = next;
            }

            return false;
        }

        public int RemoveTasks(string tag)
        {
            int count = 0;

            LinkedListNode<T> currentWaitingTask = mWaitingTasks.First;
            while (currentWaitingTask != null)
            {
                LinkedListNode<T> next = currentWaitingTask.Next;
                T task = currentWaitingTask.Value;
                if (task.Tag == tag)
                {
                    mWaitingTasks.Remove(currentWaitingTask);
                    ReferencePool.Release(task);
                    count++;
                }

                currentWaitingTask = next;
            }

            LinkedListNode<ITaskAgent<T>> currentWorkingAgent = mWorkingAgents.First;
            while (currentWorkingAgent != null)
            {
                LinkedListNode<ITaskAgent<T>> next = currentWorkingAgent.Next;
                ITaskAgent<T> workingAgent = currentWorkingAgent.Value;
                T task = workingAgent.Task;
                if (task.Tag == tag)
                {
                    workingAgent.Reset();
                    mFreeAgents.Push(workingAgent);
                    mWorkingAgents.Remove(currentWorkingAgent);
                    ReferencePool.Release(task);
                    count++;
                }

                currentWorkingAgent = next;
            }

            return count;
        }

        public int RemoveAllTasks()
        {
            int count = mWaitingTasks.Count + mWorkingAgents.Count;

            foreach (T task in mWaitingTasks)
            {
                ReferencePool.Release(task);
            }

            mWaitingTasks.Clear();

            foreach (ITaskAgent<T> workingAgent in mWorkingAgents)
            {
                T task = workingAgent.Task;
                workingAgent.Reset();
                mFreeAgents.Push(workingAgent);
                ReferencePool.Release(task);
            }

            mWorkingAgents.Clear();

            return count;
        }

        private void ProcessRunningTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<ITaskAgent<T>> current = mWorkingAgents.First;
            while (current != null)
            {
                T task = current.Value.Task;
                if (!task.Done)
                {
                    current.Value.Update(elapseSeconds, realElapseSeconds);
                    current = current.Next;
                    continue;
                }

                LinkedListNode<ITaskAgent<T>> next = current.Next;
                current.Value.Reset();
                mFreeAgents.Push(current.Value);
                mWorkingAgents.Remove(current);
                ReferencePool.Release(task);
                current = next;
            }
        }

        private void ProcessWaitingTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<T> current = mWaitingTasks.First;
            while (current != null && FreeAgentCount > 0)
            {
                ITaskAgent<T> agent = mFreeAgents.Pop();
                LinkedListNode<ITaskAgent<T>> agentNode = mWorkingAgents.AddLast(agent);
                T task = current.Value;
                LinkedListNode<T> next = current.Next;
                StartTaskStatus status = agent.Start(task);
                if (status == StartTaskStatus.Done || status == StartTaskStatus.HasToWait || status == StartTaskStatus.UnknownError)
                {
                    agent.Reset();
                    mFreeAgents.Push(agent);
                    mWorkingAgents.Remove(agentNode);
                }

                if (status == StartTaskStatus.Done || status == StartTaskStatus.CanResume || status == StartTaskStatus.UnknownError)
                {
                    mWaitingTasks.Remove(current);
                }

                if (status == StartTaskStatus.Done || status == StartTaskStatus.UnknownError)
                {
                    ReferencePool.Release(task);
                }

                current = next;
            }
        }

        public bool Paused
        {
            get
            {
                return mPaused;
            }
            set
            {
                mPaused = value;
            }
        }

        public int TotalAgentCount
        {
            get
            {
                return FreeAgentCount + WorkingAgentCount;
            }
        }

        public int FreeAgentCount
        {
            get
            {
                return mFreeAgents.Count;
            }
        }

        public int WorkingAgentCount
        {
            get
            {
                return mWorkingAgents.Count;
            }
        }

        public int WaitingTaskCount
        {
            get
            {
                return mWaitingTasks.Count;
            }
        }

        private readonly Stack<ITaskAgent<T>> mFreeAgents;
        private readonly GameFrameworkLinkedList<ITaskAgent<T>> mWorkingAgents;
        private readonly GameFrameworkLinkedList<T> mWaitingTasks;
        private bool mPaused;
    }
}