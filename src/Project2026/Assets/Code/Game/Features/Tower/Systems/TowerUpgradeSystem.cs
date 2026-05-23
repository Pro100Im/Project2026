using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerUpgradeSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _towers;

        private readonly List<GameEntity> _buffer = new(16);

        public TowerUpgradeSystem(GameContext gameContext)
        {
            _towers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Player,
                    GameMatcher.Tower,
                    GameMatcher.TowerUpgradeRequest,
                    GameMatcher.TowerUpgrade));
        }

        public void Execute()
        {
            var towers = _towers.GetEntities(_buffer);

            for (var i = 0; i < towers.Count; i++)
            {
                var tower = towers[i];
                var upgrade = tower.towerUpgrade.Value[tower.towerUpgradeRequest.Value];

                for (var j = 0; j < upgrade.Properties.Length; j++)
                {
                    var property = upgrade.Properties[j];

                    property.Apply(tower);
                }

                tower.RemoveTowerUpgradeRequest();
            }
        }
    }
}