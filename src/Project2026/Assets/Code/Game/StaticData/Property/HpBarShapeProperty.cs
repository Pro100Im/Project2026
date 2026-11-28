using Code.Game.Features.Health;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    /// <summary>
    /// Makes the entity an HP bar drawn by HpBarRenderSystem (one batched mesh for all bars, no GameObject).
    /// </summary>
    [Serializable]
    public class HpBarShapeProperty : EntityProperty
    {
        [field: SerializeField] public HpBarShape Shape { get; private set; } = new()
        {
            Width = 0.58f,
            Height = 0.023f,
            Border = 0.0015f,
            BackDelay = 0.45f,
        };

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasHpBar)
                entity.AddHpBar(Shape);

            if (!entity.hasHpBarFill)
                entity.AddHpBarFill(new HpBarFillState { Fill = 1f, BackFill = 1f, BackFrom = 1f });
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasHpBar)
                entity.RemoveHpBar();

            if (entity.hasHpBarFill)
                entity.RemoveHpBarFill();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasHpBar)
                entity.ReplaceHpBar(Shape);
        }
    }
}
