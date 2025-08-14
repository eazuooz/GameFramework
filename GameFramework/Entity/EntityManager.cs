//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using System.Collections.Generic;

namespace GameFramework.Entity
{
    internal sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        private readonly Dictionary<int, EntityInfo> mEntityInfos;
        private readonly Dictionary<string, EntityGroup> mEntityGroups;
        private readonly Dictionary<int, int> mEntitiesBeingLoaded;
        private readonly HashSet<int> mEntitiesToReleaseOnLoad;
        private readonly Queue<EntityInfo> mRecycleQueue;
        private readonly LoadAssetCallbacks mLoadAssetCallbacks;
        private IObjectPoolManager mObjectPoolManager;
        private IResourceManager mResourceManager;
        private IEntityHelper mEntityHelper;
        private int mSerial;
        private bool mIsShutdown;
        private EventHandler<ShowEntitySuccessEventArgs> mShowEntitySuccessEventHandler;
        private EventHandler<ShowEntityFailureEventArgs> mShowEntityFailureEventHandler;
        private EventHandler<ShowEntityUpdateEventArgs> mShowEntityUpdateEventHandler;
        private EventHandler<ShowEntityDependencyAssetEventArgs> mShowEntityDependencyAssetEventHandler;
        private EventHandler<HideEntityCompleteEventArgs> mHideEntityCompleteEventHandler;

        public EntityManager()
        {
            mEntityInfos = new Dictionary<int, EntityInfo>();
            mEntityGroups = new Dictionary<string, EntityGroup>(StringComparer.Ordinal);
            mEntitiesBeingLoaded = new Dictionary<int, int>();
            mEntitiesToReleaseOnLoad = new HashSet<int>();
            mRecycleQueue = new Queue<EntityInfo>();
            mLoadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback, LoadAssetUpdateCallback, LoadAssetDependencyAssetCallback);
            mObjectPoolManager = null;
            mResourceManager = null;
            mEntityHelper = null;
            mSerial = 0;
            mIsShutdown = false;
            mShowEntitySuccessEventHandler = null;
            mShowEntityFailureEventHandler = null;
            mShowEntityUpdateEventHandler = null;
            mShowEntityDependencyAssetEventHandler = null;
            mHideEntityCompleteEventHandler = null;
        }

        public int EntityCount
        {
            get
            {
                return mEntityInfos.Count;
            }
        }

        public int EntityGroupCount
        {
            get
            {
                return mEntityGroups.Count;
            }
        }

        public event EventHandler<ShowEntitySuccessEventArgs> ShowEntitySuccess
        {
            add
            {
                mShowEntitySuccessEventHandler += value;
            }
            remove
            {
                mShowEntitySuccessEventHandler -= value;
            }
        }

        public event EventHandler<ShowEntityFailureEventArgs> ShowEntityFailure
        {
            add
            {
                mShowEntityFailureEventHandler += value;
            }
            remove
            {
                mShowEntityFailureEventHandler -= value;
            }
        }

        public event EventHandler<ShowEntityUpdateEventArgs> ShowEntityUpdate
        {
            add
            {
                mShowEntityUpdateEventHandler += value;
            }
            remove
            {
                mShowEntityUpdateEventHandler -= value;
            }
        }

        public event EventHandler<ShowEntityDependencyAssetEventArgs> ShowEntityDependencyAsset
        {
            add
            {
                mShowEntityDependencyAssetEventHandler += value;
            }
            remove
            {
                mShowEntityDependencyAssetEventHandler -= value;
            }
        }

        public event EventHandler<HideEntityCompleteEventArgs> HideEntityComplete
        {
            add
            {
                mHideEntityCompleteEventHandler += value;
            }
            remove
            {
                mHideEntityCompleteEventHandler -= value;
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            while (mRecycleQueue.Count > 0)
            {
                EntityInfo entityInfo = mRecycleQueue.Dequeue();
                IEntity entity = entityInfo.Entity;
                EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
                if (entityGroup == null)
                {
                    throw new GameFrameworkException("Entity group is invalid.");
                }

                entityInfo.Status = EntityStatus.WillRecycle;
                entity.OnRecycle();
                entityInfo.Status = EntityStatus.Recycled;
                entityGroup.UnspawnEntity(entity);
                ReferencePool.Release(entityInfo);
            }

            foreach (KeyValuePair<string, EntityGroup> entityGroup in mEntityGroups)
            {
                entityGroup.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        internal override void Shutdown()
        {
            mIsShutdown = true;
            HideAllLoadedEntities();
            mEntityGroups.Clear();
            mEntitiesBeingLoaded.Clear();
            mEntitiesToReleaseOnLoad.Clear();
            mRecycleQueue.Clear();
        }

        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new GameFrameworkException("Object pool manager is invalid.");
            }

            mObjectPoolManager = objectPoolManager;
        }

        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            mResourceManager = resourceManager;
        }

        public void SetEntityHelper(IEntityHelper entityHelper)
        {
            if (entityHelper == null)
            {
                throw new GameFrameworkException("Entity helper is invalid.");
            }

            mEntityHelper = entityHelper;
        }

        public bool HasEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            return mEntityGroups.ContainsKey(entityGroupName);
        }

        public IEntityGroup GetEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            EntityGroup entityGroup = null;
            if (mEntityGroups.TryGetValue(entityGroupName, out entityGroup))
            {
                return entityGroup;
            }

            return null;
        }

        public IEntityGroup[] GetAllEntityGroups()
        {
            int index = 0;
            IEntityGroup[] results = new IEntityGroup[mEntityGroups.Count];
            foreach (KeyValuePair<string, EntityGroup> entityGroup in mEntityGroups)
            {
                results[index++] = entityGroup.Value;
            }

            return results;
        }

        public void GetAllEntityGroups(List<IEntityGroup> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, EntityGroup> entityGroup in mEntityGroups)
            {
                results.Add(entityGroup.Value);
            }
        }

        public bool AddEntityGroup(string entityGroupName, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime, int instancePriority, IEntityGroupHelper entityGroupHelper)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            if (entityGroupHelper == null)
            {
                throw new GameFrameworkException("Entity group helper is invalid.");
            }

            if (mObjectPoolManager == null)
            {
                throw new GameFrameworkException("You must set object pool manager first.");
            }

            if (HasEntityGroup(entityGroupName))
            {
                return false;
            }

            mEntityGroups.Add(entityGroupName, new EntityGroup(entityGroupName, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime, instancePriority, entityGroupHelper, mObjectPoolManager));

            return true;
        }

        public bool HasEntity(int entityId)
        {
            return mEntityInfos.ContainsKey(entityId);
        }

        public bool HasEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            foreach (KeyValuePair<int, EntityInfo> entityInfo in mEntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return true;
                }
            }

            return false;
        }

        public IEntity GetEntity(int entityId)
        {
            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                return null;
            }

            return entityInfo.Entity;
        }

        public IEntity GetEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            foreach (KeyValuePair<int, EntityInfo> entityInfo in mEntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return entityInfo.Value.Entity;
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
            foreach (KeyValuePair<int, EntityInfo> entityInfo in mEntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
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
            foreach (KeyValuePair<int, EntityInfo> entityInfo in mEntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
                }
            }
        }

        public IEntity[] GetAllLoadedEntities()
        {
            int index = 0;
            IEntity[] results = new IEntity[mEntityInfos.Count];
            foreach (KeyValuePair<int, EntityInfo> entityInfo in mEntityInfos)
            {
                results[index++] = entityInfo.Value.Entity;
            }

            return results;
        }

        public void GetAllLoadedEntities(List<IEntity> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in mEntityInfos)
            {
                results.Add(entityInfo.Value.Entity);
            }
        }

        public int[] GetAllLoadingEntityIds()
        {
            int index = 0;
            int[] results = new int[mEntitiesBeingLoaded.Count];
            foreach (KeyValuePair<int, int> entityBeingLoaded in mEntitiesBeingLoaded)
            {
                results[index++] = entityBeingLoaded.Key;
            }

            return results;
        }

        public void GetAllLoadingEntityIds(List<int> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, int> entityBeingLoaded in mEntitiesBeingLoaded)
            {
                results.Add(entityBeingLoaded.Key);
            }
        }

        public bool IsLoadingEntity(int entityId)
        {
            return mEntitiesBeingLoaded.ContainsKey(entityId);
        }

        public bool IsValidEntity(IEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return HasEntity(entity.Id);
        }

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName)
        {
            ShowEntity(entityId, entityAssetName, entityGroupName, Constant.DefaultPriority, null);
        }

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, int priority)
        {
            ShowEntity(entityId, entityAssetName, entityGroupName, priority, null);
        }

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, object userData)
        {
            ShowEntity(entityId, entityAssetName, entityGroupName, Constant.DefaultPriority, userData);
        }

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, int priority, object userData)
        {
            if (mResourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (mEntityHelper == null)
            {
                throw new GameFrameworkException("You must set entity helper first.");
            }

            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            if (HasEntity(entityId))
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity id '{0}' is already exist.", entityId));
            }

            if (IsLoadingEntity(entityId))
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity '{0}' is already being loaded.", entityId));
            }

            EntityGroup entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);
            if (entityGroup == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity group '{0}' is not exist.", entityGroupName));
            }

            EntityInstanceObject entityInstanceObject = entityGroup.SpawnEntityInstanceObject(entityAssetName);
            if (entityInstanceObject == null)
            {
                int serialId = ++mSerial;
                mEntitiesBeingLoaded.Add(entityId, serialId);
                mResourceManager.LoadAsset(entityAssetName, priority, mLoadAssetCallbacks, ShowEntityInfo.Create(serialId, entityId, entityGroup, userData));
                return;
            }

            InternalShowEntity(entityId, entityAssetName, entityGroup, entityInstanceObject.Target, false, 0f, userData);
        }

        public void HideEntity(int entityId)
        {
            HideEntity(entityId, null);
        }

        public void HideEntity(int entityId, object userData)
        {
            if (IsLoadingEntity(entityId))
            {
                mEntitiesToReleaseOnLoad.Add(mEntitiesBeingLoaded[entityId]);
                mEntitiesBeingLoaded.Remove(entityId);
                return;
            }

            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find entity '{0}'.", entityId));
            }

            InternalHideEntity(entityInfo, userData);
        }

        public void HideEntity(IEntity entity)
        {
            HideEntity(entity, null);
        }

        public void HideEntity(IEntity entity, object userData)
        {
            if (entity == null)
            {
                throw new GameFrameworkException("Entity is invalid.");
            }

            HideEntity(entity.Id, userData);
        }

        public void HideAllLoadedEntities()
        {
            HideAllLoadedEntities(null);
        }

        public void HideAllLoadedEntities(object userData)
        {
            while (mEntityInfos.Count > 0)
            {
                foreach (KeyValuePair<int, EntityInfo> entityInfo in mEntityInfos)
                {
                    InternalHideEntity(entityInfo.Value, userData);
                    break;
                }
            }
        }

        public void HideAllLoadingEntities()
        {
            foreach (KeyValuePair<int, int> entityBeingLoaded in mEntitiesBeingLoaded)
            {
                mEntitiesToReleaseOnLoad.Add(entityBeingLoaded.Value);
            }

            mEntitiesBeingLoaded.Clear();
        }

        public IEntity GetParentEntity(int childEntityId)
        {
            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.", childEntityId));
            }

            return childEntityInfo.ParentEntity;
        }

        public IEntity GetParentEntity(IEntity childEntity)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            return GetParentEntity(childEntity.Id);
        }

        public int GetChildEntityCount(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.ChildEntityCount;
        }

        public IEntity GetChildEntity(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.GetChildEntity();
        }

        public IEntity GetChildEntity(IEntity parentEntity)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            return GetChildEntity(parentEntity.Id);
        }

        public IEntity[] GetChildEntities(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.GetChildEntities();
        }

        public void GetChildEntities(int parentEntityId, List<IEntity> results)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            parentEntityInfo.GetChildEntities(results);
        }

        public IEntity[] GetChildEntities(IEntity parentEntity)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            return GetChildEntities(parentEntity.Id);
        }

        public void GetChildEntities(IEntity parentEntity, List<IEntity> results)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            GetChildEntities(parentEntity.Id, results);
        }

        public void AttachEntity(int childEntityId, int parentEntityId)
        {
            AttachEntity(childEntityId, parentEntityId, null);
        }

        public void AttachEntity(int childEntityId, int parentEntityId, object userData)
        {
            if (childEntityId == parentEntityId)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when child entity id equals to parent entity id '{0}'.", parentEntityId));
            }

            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.", childEntityId));
            }

            if (childEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when child entity status is '{0}'.", childEntityInfo.Status));
            }

            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            if (parentEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when parent entity status is '{0}'.", parentEntityInfo.Status));
            }

            IEntity childEntity = childEntityInfo.Entity;
            IEntity parentEntity = parentEntityInfo.Entity;
            DetachEntity(childEntity.Id, userData);
            childEntityInfo.ParentEntity = parentEntity;
            parentEntityInfo.AddChildEntity(childEntity);
            parentEntity.OnAttached(childEntity, userData);
            childEntity.OnAttachTo(parentEntity, userData);
        }

        public void AttachEntity(int childEntityId, IEntity parentEntity)
        {
            AttachEntity(childEntityId, parentEntity, null);
        }

        public void AttachEntity(int childEntityId, IEntity parentEntity, object userData)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            AttachEntity(childEntityId, parentEntity.Id, userData);
        }

        public void AttachEntity(IEntity childEntity, int parentEntityId)
        {
            AttachEntity(childEntity, parentEntityId, null);
        }

        public void AttachEntity(IEntity childEntity, int parentEntityId, object userData)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            AttachEntity(childEntity.Id, parentEntityId, userData);
        }

        public void AttachEntity(IEntity childEntity, IEntity parentEntity)
        {
            AttachEntity(childEntity, parentEntity, null);
        }

        public void AttachEntity(IEntity childEntity, IEntity parentEntity, object userData)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            AttachEntity(childEntity.Id, parentEntity.Id, userData);
        }

        public void DetachEntity(int childEntityId)
        {
            DetachEntity(childEntityId, null);
        }

        public void DetachEntity(int childEntityId, object userData)
        {
            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.", childEntityId));
            }

            IEntity parentEntity = childEntityInfo.ParentEntity;
            if (parentEntity == null)
            {
                return;
            }

            EntityInfo parentEntityInfo = GetEntityInfo(parentEntity.Id);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntity.Id));
            }

            IEntity childEntity = childEntityInfo.Entity;
            childEntityInfo.ParentEntity = null;
            parentEntityInfo.RemoveChildEntity(childEntity);
            parentEntity.OnDetached(childEntity, userData);
            childEntity.OnDetachFrom(parentEntity, userData);
        }

        public void DetachEntity(IEntity childEntity)
        {
            DetachEntity(childEntity, null);
        }

        public void DetachEntity(IEntity childEntity, object userData)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            DetachEntity(childEntity.Id, userData);
        }

        public void DetachChildEntities(int parentEntityId)
        {
            DetachChildEntities(parentEntityId, null);
        }

        public void DetachChildEntities(int parentEntityId, object userData)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            while (parentEntityInfo.ChildEntityCount > 0)
            {
                IEntity childEntity = parentEntityInfo.GetChildEntity();
                DetachEntity(childEntity.Id, userData);
            }
        }

        public void DetachChildEntities(IEntity parentEntity)
        {
            DetachChildEntities(parentEntity, null);
        }

        public void DetachChildEntities(IEntity parentEntity, object userData)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            DetachChildEntities(parentEntity.Id, userData);
        }

        private EntityInfo GetEntityInfo(int entityId)
        {
            EntityInfo entityInfo = null;
            if (mEntityInfos.TryGetValue(entityId, out entityInfo))
            {
                return entityInfo;
            }

            return null;
        }

        private void InternalShowEntity(int entityId, string entityAssetName, EntityGroup entityGroup, object entityInstance, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IEntity entity = mEntityHelper.CreateEntity(entityInstance, entityGroup, userData);
                if (entity == null)
                {
                    throw new GameFrameworkException("Can not create entity in entity helper.");
                }

                EntityInfo entityInfo = EntityInfo.Create(entity);
                mEntityInfos.Add(entityId, entityInfo);
                entityInfo.Status = EntityStatus.WillInit;
                entity.OnInit(entityId, entityAssetName, entityGroup, isNewInstance, userData);
                entityInfo.Status = EntityStatus.Inited;
                entityGroup.AddEntity(entity);
                entityInfo.Status = EntityStatus.WillShow;
                entity.OnShow(userData);
                entityInfo.Status = EntityStatus.Showed;

                if (mShowEntitySuccessEventHandler != null)
                {
                    ShowEntitySuccessEventArgs showEntitySuccessEventArgs = ShowEntitySuccessEventArgs.Create(entity, duration, userData);
                    mShowEntitySuccessEventHandler(this, showEntitySuccessEventArgs);
                    ReferencePool.Release(showEntitySuccessEventArgs);
                }
            }
            catch (Exception exception)
            {
                if (mShowEntityFailureEventHandler != null)
                {
                    ShowEntityFailureEventArgs showEntityFailureEventArgs = ShowEntityFailureEventArgs.Create(entityId, entityAssetName, entityGroup.Name, exception.ToString(), userData);
                    mShowEntityFailureEventHandler(this, showEntityFailureEventArgs);
                    ReferencePool.Release(showEntityFailureEventArgs);
                    return;
                }

                throw;
            }
        }

        private void InternalHideEntity(EntityInfo entityInfo, object userData)
        {
            while (entityInfo.ChildEntityCount > 0)
            {
                IEntity childEntity = entityInfo.GetChildEntity();
                HideEntity(childEntity.Id, userData);
            }

            if (entityInfo.Status == EntityStatus.Hidden)
            {
                return;
            }

            IEntity entity = entityInfo.Entity;
            DetachEntity(entity.Id, userData);
            entityInfo.Status = EntityStatus.WillHide;
            entity.OnHide(mIsShutdown, userData);
            entityInfo.Status = EntityStatus.Hidden;

            EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
            if (entityGroup == null)
            {
                throw new GameFrameworkException("Entity group is invalid.");
            }

            entityGroup.RemoveEntity(entity);
            if (!mEntityInfos.Remove(entity.Id))
            {
                throw new GameFrameworkException("Entity info is unmanaged.");
            }

            if (mHideEntityCompleteEventHandler != null)
            {
                HideEntityCompleteEventArgs hideEntityCompleteEventArgs = HideEntityCompleteEventArgs.Create(entity.Id, entity.EntityAssetName, entityGroup, userData);
                mHideEntityCompleteEventHandler(this, hideEntityCompleteEventArgs);
                ReferencePool.Release(hideEntityCompleteEventArgs);
            }

            mRecycleQueue.Enqueue(entityInfo);
        }

        private void LoadAssetSuccessCallback(string entityAssetName, object entityAsset, float duration, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (mEntitiesToReleaseOnLoad.Contains(showEntityInfo.SerialId))
            {
                mEntitiesToReleaseOnLoad.Remove(showEntityInfo.SerialId);
                ReferencePool.Release(showEntityInfo);
                mEntityHelper.ReleaseEntity(entityAsset, null);
                return;
            }

            mEntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
            EntityInstanceObject entityInstanceObject = EntityInstanceObject.Create(entityAssetName, entityAsset, mEntityHelper.InstantiateEntity(entityAsset), mEntityHelper);
            showEntityInfo.EntityGroup.RegisterEntityInstanceObject(entityInstanceObject, true);

            InternalShowEntity(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup, entityInstanceObject.Target, true, duration, showEntityInfo.UserData);
            ReferencePool.Release(showEntityInfo);
        }

        private void LoadAssetFailureCallback(string entityAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (mEntitiesToReleaseOnLoad.Contains(showEntityInfo.SerialId))
            {
                mEntitiesToReleaseOnLoad.Remove(showEntityInfo.SerialId);
                return;
            }

            mEntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
            string appendErrorMessage = Utility.Text.Format("Load entity failure, asset name '{0}', status '{1}', error message '{2}'.", entityAssetName, status, errorMessage);
            if (mShowEntityFailureEventHandler != null)
            {
                ShowEntityFailureEventArgs showEntityFailureEventArgs = ShowEntityFailureEventArgs.Create(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, appendErrorMessage, showEntityInfo.UserData);
                mShowEntityFailureEventHandler(this, showEntityFailureEventArgs);
                ReferencePool.Release(showEntityFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }

        private void LoadAssetUpdateCallback(string entityAssetName, float progress, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (mShowEntityUpdateEventHandler != null)
            {
                ShowEntityUpdateEventArgs showEntityUpdateEventArgs = ShowEntityUpdateEventArgs.Create(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, progress, showEntityInfo.UserData);
                mShowEntityUpdateEventHandler(this, showEntityUpdateEventArgs);
                ReferencePool.Release(showEntityUpdateEventArgs);
            }
        }

        private void LoadAssetDependencyAssetCallback(string entityAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (mShowEntityDependencyAssetEventHandler != null)
            {
                ShowEntityDependencyAssetEventArgs showEntityDependencyAssetEventArgs = ShowEntityDependencyAssetEventArgs.Create(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, dependencyAssetName, loadedCount, totalCount, showEntityInfo.UserData);
                mShowEntityDependencyAssetEventHandler(this, showEntityDependencyAssetEventArgs);
                ReferencePool.Release(showEntityDependencyAssetEventArgs);
            }
        }
    }
}
