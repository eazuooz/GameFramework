//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.ObjectPool;
using System.Collections.Generic;

namespace GameFramework.Entity
{
    internal sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        private sealed class EntityGroup : IEntityGroup
        {
            private readonly string mName;
            private readonly IEntityGroupHelper mEntityGroupHelper;
            private readonly IObjectPool<EntityInstanceObject> mInstancePool;
            private readonly GameFrameworkLinkedList<IEntity> mEntities;
            private LinkedListNode<IEntity> mCachedNode;

            public EntityGroup(string name, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime, int instancePriority, IEntityGroupHelper entityGroupHelper, IObjectPoolManager objectPoolManager)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Entity group name is invalid.");
                }

                if (entityGroupHelper == null)
                {
                    throw new GameFrameworkException("Entity group helper is invalid.");
                }

                mName = name;
                mEntityGroupHelper = entityGroupHelper;
                mInstancePool = objectPoolManager.CreateSingleSpawnObjectPool<EntityInstanceObject>(Utility.Text.Format("Entity Instance Pool ({0})", name), instanceCapacity, instanceExpireTime, instancePriority);
                mInstancePool.AutoReleaseInterval = instanceAutoReleaseInterval;
                mEntities = new GameFrameworkLinkedList<IEntity>();
                mCachedNode = null;
            }

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public int EntityCount
            {
                get
                {
                    return mEntities.Count;
                }
            }

            public float InstanceAutoReleaseInterval
            {
                get
                {
                    return mInstancePool.AutoReleaseInterval;
                }
                set
                {
                    mInstancePool.AutoReleaseInterval = value;
                }
            }

            public int InstanceCapacity
            {
                get
                {
                    return mInstancePool.Capacity;
                }
                set
                {
                    mInstancePool.Capacity = value;
                }
            }

            public float InstanceExpireTime
            {
                get
                {
                    return mInstancePool.ExpireTime;
                }
                set
                {
                    mInstancePool.ExpireTime = value;
                }
            }

            public int InstancePriority
            {
                get
                {
                    return mInstancePool.Priority;
                }
                set
                {
                    mInstancePool.Priority = value;
                }
            }

            public IEntityGroupHelper Helper
            {
                get
                {
                    return mEntityGroupHelper;
                }
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                LinkedListNode<IEntity> current = mEntities.First;
                while (current != null)
                {
                    mCachedNode = current.Next;
                    current.Value.OnUpdate(elapseSeconds, realElapseSeconds);
                    current = mCachedNode;
                    mCachedNode = null;
                }
            }

            public bool HasEntity(int entityId)
            {
                foreach (IEntity entity in mEntities)
                {
                    if (entity.Id == entityId)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool HasEntity(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                foreach (IEntity entity in mEntities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        return true;
                    }
                }

                return false;
            }

            public IEntity GetEntity(int entityId)
            {
                foreach (IEntity entity in mEntities)
                {
                    if (entity.Id == entityId)
                    {
                        return entity;
                    }
                }

                return null;
            }

            public IEntity GetEntity(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                foreach (IEntity entity in mEntities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        return entity;
                    }
                }

                return null;
            }

            public IEntity[] GetEntities(string entityAssetName)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                List<IEntity> results = new List<IEntity>();
                foreach (IEntity entity in mEntities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        results.Add(entity);
                    }
                }

                return results.ToArray();
            }

            public void GetEntities(string entityAssetName, List<IEntity> results)
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity entity in mEntities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        results.Add(entity);
                    }
                }
            }

            public IEntity[] GetAllEntities()
            {
                List<IEntity> results = new List<IEntity>();
                foreach (IEntity entity in mEntities)
                {
                    results.Add(entity);
                }

                return results.ToArray();
            }

            public void GetAllEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity entity in mEntities)
                {
                    results.Add(entity);
                }
            }

            public void AddEntity(IEntity entity)
            {
                mEntities.AddLast(entity);
            }

            public void RemoveEntity(IEntity entity)
            {
                if (mCachedNode != null && mCachedNode.Value == entity)
                {
                    mCachedNode = mCachedNode.Next;
                }

                if (!mEntities.Remove(entity))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Entity group '{0}' not exists specified entity '[{1}]{2}'.", mName, entity.Id, entity.EntityAssetName));
                }
            }

            public void RegisterEntityInstanceObject(EntityInstanceObject obj, bool spawned)
            {
                mInstancePool.Register(obj, spawned);
            }

            public EntityInstanceObject SpawnEntityInstanceObject(string name)
            {
                return mInstancePool.Spawn(name);
            }

            public void UnspawnEntity(IEntity entity)
            {
                mInstancePool.Unspawn(entity.Handle);
            }

            public void SetEntityInstanceLocked(object entityInstance, bool locked)
            {
                if (entityInstance == null)
                {
                    throw new GameFrameworkException("Entity instance is invalid.");
                }

                mInstancePool.SetLocked(entityInstance, locked);
            }

            public void SetEntityInstancePriority(object entityInstance, int priority)
            {
                if (entityInstance == null)
                {
                    throw new GameFrameworkException("Entity instance is invalid.");
                }

                mInstancePool.SetPriority(entityInstance, priority);
            }
        }
    }
}
