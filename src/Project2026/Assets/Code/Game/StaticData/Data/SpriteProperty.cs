using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class SpriteProperty : EntityProperty
    {
        [field: SerializeField] public Sprite _sprite { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasSprite)
                entity.AddSprite(_sprite);

            if(entity.hasSpriteRenderer)
                entity.spriteRenderer.Value.sprite = _sprite;
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasSprite)
                entity.RemoveSprite();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasSprite)
                entity.ReplaceSprite(_sprite);

            if (entity.hasSpriteRenderer)
                entity.spriteRenderer.Value.sprite = _sprite;
        }
    }
}