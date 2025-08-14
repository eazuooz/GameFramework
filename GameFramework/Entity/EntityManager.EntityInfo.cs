//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.Entity
{
    internal sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        private sealed class EntityInfo : IReference
        {
            private IEntity mEntity;
            private EntityStatus mStatus;
            private IEntity mParentEntity;
            private List<IEntity> mChildEntities;

            public EntityInfo()
            {
                mEntity = null;
                mStatus = EntityStatus.Unknown;
                mParentEntity = null;
                mChildEntities = new List<IEntity>();
            }

            public IEntity Entity
            {
                get
                {
                    return mEntity;
                }
            }

            public EntityStatus Status
            {
                get
                {
                    return mStatus;
                }
                set
                {
                    mStatus = value;
                }
            }

            public IEntity ParentEntity
            {
                get
                {
                    return mParentEntity;
                }
                set
                {
                    mParentEntity = value;
                }
            }

            public int ChildEntityCount
            {
                get
                {
                    return mChildEntities.Count;
                }
            }

            public static EntityInfo Create(IEntity entity)
            {
                if (entity == null)
                {
                    throw new GameFrameworkException("Entity is invalid.");
                }

                EntityInfo entityInfo = ReferencePool.Acquire<EntityInfo>();
                entityInfo.mEntity = entity;
                entityInfo.mStatus = EntityStatus.WillInit;
                return entityInfo;
            }

            public void Clear()
            {
                mEntity = null;
                mStatus = EntityStatus.Unknown;
                mParentEntity = null;
                mChildEntities.Clear();
            }

            public IEntity GetChildEntity()
            {
                return mChildEntities.Count > 0 ? mChildEntities[0] : null;
            }

            public IEntity[] GetChildEntities()
            {
                return mChildEntities.ToArray();
            }

            public void GetChildEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity childEntity in mChildEntities)
                {
                    results.Add(childEntity);
                }
            }

            public void AddChildEntity(IEntity childEntity)
            {
                if (mChildEntities.Contains(childEntity))
                {
                    throw new GameFrameworkException("Can not add child entity which is already exist.");
                }

                mChildEntities.Add(childEntity);
            }

            public void RemoveChildEntity(IEntity childEntity)
            {
                if (!mChildEntities.Remove(childEntity))
                {
                    throw new GameFrameworkException("Can not remove child entity which is not exist.");
                }
            }
        }
    }
}
