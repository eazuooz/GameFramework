//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Entity
{
    internal sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        private sealed class ShowEntityInfo : IReference
        {
            private int mSerialId;
            private int mEntityId;
            private EntityGroup mEntityGroup;
            private object mUserData;

            public ShowEntityInfo()
            {
                mSerialId = 0;
                mEntityId = 0;
                mEntityGroup = null;
                mUserData = null;
            }

            public int SerialId
            {
                get
                {
                    return mSerialId;
                }
            }

            public int EntityId
            {
                get
                {
                    return mEntityId;
                }
            }

            public EntityGroup EntityGroup
            {
                get
                {
                    return mEntityGroup;
                }
            }

            public object UserData
            {
                get
                {
                    return mUserData;
                }
            }

            public static ShowEntityInfo Create(int serialId, int entityId, EntityGroup entityGroup, object userData)
            {
                ShowEntityInfo showEntityInfo = ReferencePool.Acquire<ShowEntityInfo>();
                showEntityInfo.mSerialId = serialId;
                showEntityInfo.mEntityId = entityId;
                showEntityInfo.mEntityGroup = entityGroup;
                showEntityInfo.mUserData = userData;
                return showEntityInfo;
            }

            public void Clear()
            {
                mSerialId = 0;
                mEntityId = 0;
                mEntityGroup = null;
                mUserData = null;
            }
        }
    }
}
