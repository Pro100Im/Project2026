using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerBuildSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _places;

        private readonly List<GameEntity> _buffer = new(32);

        public TowerBuildSystem(GameContext gameContext)
        {
            _places = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Player,
                GameMatcher.TowerPlace,
                GameMatcher.TowerBuildRequest));
        } 

        public void Execute()
        {
            foreach (var place in _places.GetEntities(_buffer))
            {
                place.AddDamage(10);
                place.AddAttackDuration(0.5f);
                place.AddAttackCooldown(1f);
                place.AddRange(5f);

                place.isTowerBuildRequest = false;
                place.isTowerPlace = false;
                place.isTower = true;
                place.isMeleeAttack = false;
                place.isAttackAvailable = true;
            }
        }
    }
}