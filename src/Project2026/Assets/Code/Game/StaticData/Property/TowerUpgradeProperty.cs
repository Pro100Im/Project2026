using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class TowerUpgradeProperty : EntityProperty
    {
        [field: SerializeField] public int[] Prices { get; private set; }
        [field: SerializeField] public Sprite[] UpgradeIcons { get; private set; }
        [field: SerializeField] public EntityConfig[] Upgrades { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasTowerUpgradePrice)
                entity.AddTowerUpgradePrice(Prices);

            if (!entity.hasTowerUpgrade)
                entity.AddTowerUpgrade(Upgrades);

            if (!entity.hasTowerUpgradeIcon)
                entity.AddTowerUpgradeIcon(UpgradeIcons);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasTowerUpgradePrice)
                entity.RemoveTowerUpgradePrice();

            if (entity.hasTowerUpgrade)
                entity.RemoveTowerUpgrade();

            if (entity.hasTowerUpgradeIcon) 
                entity.RemoveTowerUpgradeIcon();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasTowerUpgradePrice)
                entity.ReplaceTowerUpgradePrice(Prices);

            if (entity.hasTowerUpgrade)
                entity.ReplaceTowerUpgrade(Upgrades);

            if (entity.hasTowerUpgradeIcon)
                entity.ReplaceTowerUpgradeIcon(UpgradeIcons);
        }
    }
}