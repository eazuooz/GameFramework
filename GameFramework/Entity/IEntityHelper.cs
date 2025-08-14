//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

namespace GameFramework.Entity
{
    public interface IEntityHelper
    {
        object InstantiateEntity(object entityAsset);

        IEntity CreateEntity(object entityInstance, IEntityGroup entityGroup, object userData);

        void ReleaseEntity(object entityAsset, object entityInstance);
    }
}
