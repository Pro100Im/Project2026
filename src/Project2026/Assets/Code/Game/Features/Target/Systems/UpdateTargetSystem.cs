using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Target.Systems
{
    public class UpdateTargetSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _entities;

        private readonly List<GameEntity> _buffer = new(32);

        public UpdateTargetSystem(GameContext gameContext, TargetService targetService)
        {
            _targetService = targetService;

            _entities = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Range,
                GameMatcher.TargetId,
                GameMatcher.TargetPoint,
                GameMatcher.AttackerPoint,
                GameMatcher.SpriteRenderer));
        }

        // To do
        public void Execute()
        {
            foreach (var entity in _entities.GetEntities(_buffer))
            {
                if (entity.isDead)
                    continue;

                var target = GetGameEntityById.Get(entity.targetId.Value);

                var ba = entity.spriteRenderer.Value.bounds;
                var bb = target.spriteRenderer.Value.bounds;

                var closestA = ba.ClosestPoint(bb.center);
                var closestB = bb.ClosestPoint(ba.center);

                var distance = _targetService.GetDistanceBetweenEntities(closestA, closestB);

                if (distance > entity.range.Value || target.isDead)
                {
                    entity.RemoveTargetId();
                    entity.RemoveAttackerPoint();
                    entity.RemoveTargetPoint();
                }
                else
                {
                    entity.ReplaceAttackerPoint(closestA);
                    entity.ReplaceTargetPoint(closestB);
                }
            }
        }
    }
}