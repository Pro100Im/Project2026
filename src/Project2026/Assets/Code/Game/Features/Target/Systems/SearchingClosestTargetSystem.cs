using Code.Game.Features.Target.Services;
using Entitas;

namespace Code.Game.Features.Target.Systems
{
    // To do
    public class SearchingClosestTargetSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _warriors;
        private readonly IGroup<GameEntity> _enemies;

        public SearchingClosestTargetSystem(GameContext gameContext, TargetService targetService)
        {
            _targetService = targetService;

            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Targetable,
                GameMatcher.SpriteRenderer,
                GameMatcher.Enemy));

            _warriors = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Targetable,
                GameMatcher.SpriteRenderer,
                GameMatcher.Player));
        }

        public void Execute()
        {
            foreach (var enemy in _enemies)
            {
                if (enemy.hasTargetId)
                    continue;

                foreach (var warrior in _warriors)
                {
                    if (warrior.isDead)
                        continue;

                    var ba = enemy.spriteRenderer.Value.bounds;
                    var bb = warrior.spriteRenderer.Value.bounds;

                    var closestA = _targetService.ClosestPointAABB2D(ba.center, bb.center, ba.size);
                    var closestB = _targetService.ClosestPointAABB2D(bb.center, ba.center, bb.size);
                    var distance = _targetService.GetDistanceBetweenEntities(ba, bb, closestA, closestB);

                    if (distance <= enemy.range.Value)
                    {
                        enemy.AddTargetId(warrior.id.Value);
                        enemy.AddAttackerPoint(closestA);
                        enemy.AddTargetPoint(closestB);
                    }

                    break;
                }
            }

            foreach (var warrior in _warriors)
            {
                if (warrior.hasTargetId)
                    continue;

                if (!warrior.hasRange)
                    continue;

                foreach (var enemy in _enemies)
                {
                    if (enemy.isDead)
                        continue;

                    var ba = enemy.spriteRenderer.Value.bounds;
                    var bb = warrior.spriteRenderer.Value.bounds;

                    var closestA = _targetService.ClosestPointAABB2D(ba.center, bb.center, ba.size);
                    var closestB = _targetService.ClosestPointAABB2D(bb.center, ba.center, bb.size);
                    var distance = _targetService.GetDistanceBetweenEntities(ba, bb, closestA, closestB);

                    if (distance <= warrior.range.Value)
                    {  
                        warrior.AddTargetId(enemy.id.Value);
                        warrior.AddAttackerPoint(closestA);
                        warrior.AddTargetPoint(closestB);
                    }

                    break;
                }
            }
        }
    }
}