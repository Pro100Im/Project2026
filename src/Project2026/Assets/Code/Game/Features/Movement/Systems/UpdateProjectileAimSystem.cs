using Code.Game.Common.Entity;
using Code.Game.Features.Level;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Movement.Systems
{
    public class UpdateProjectileAimSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _projectiles;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(LevelBuffers.DefaultCapacity);

        public UpdateProjectileAimSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _projectiles = gameContext.GetGroup(GameMatcher.AllOf(
                GameMatcher.TrajectoryMovement,
                GameMatcher.TargetId,
                GameMatcher.AttackerPoint,
                GameMatcher.TargetPoint));

            _maps = gameContext.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var arena = mapEntity.arena.Value;
            var projectiles = _projectiles.GetEntities(_buffer);

            for (var i = 0; i < projectiles.Count; i++)
            {
                var projectile = projectiles[i];
                var target = GetGameEntityById.Get(projectile.targetId.Value);

                if (target == null || target.isDead || !target.hasCurrentCell)
                    continue;

                var firePoint = projectile.attackerPoint.Value;
                var targetPoint = TargetService.GetClosestPoint(target, arena, firePoint);

                projectile.ReplaceTargetPoint(targetPoint);
            }
        }
    }
}
