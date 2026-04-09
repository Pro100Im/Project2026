using Code.Game.Features.Attack;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    // To do
    public class SearchingClosestTargetSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _warriors;
        private readonly IGroup<GameEntity> _enemies;

        public SearchingClosestTargetSystem(GameContext gameContext)
        {
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

                    var closestB = ClosestPointAABB2D(ba.center, bb.center, bb.size);
                    var closestA = ClosestPointAABB2D(bb.center, ba.center, ba.size);
                    var distance = GetDistanceBetweenEntities(ba, bb, closestB, closestA);

                    if (distance <= enemy.range.Value)
                    {
                        var attackDirection = GetAttackDirection(closestB, closestA);

                        enemy.AddTargetId(warrior.id.Value);
                        enemy.AddAttackDirection(attackDirection);
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

                    var closestB = ClosestPointAABB2D(ba.center, bb.center, bb.size);
                    var closestA = ClosestPointAABB2D(bb.center, ba.center, ba.size);
                    var distance = GetDistanceBetweenEntities(ba, bb, closestB, closestA);

                    if (distance <= warrior.range.Value)
                    {
                        var attackDirection = GetAttackDirection(closestB, closestA);

                        warrior.AddTargetId(enemy.id.Value);
                        warrior.AddAttackDirection(attackDirection);
                    }

                    break;
                }
            }
        }

        private float GetDistanceBetweenEntities(Bounds ba, Bounds bb, Vector2 closestB, Vector2 closestA)
        {
            var dist1 = Vector2.Distance(ba.center, closestB);
            var dist2 = Vector2.Distance(bb.center, closestA);

            return Mathf.Min(dist1, dist2);
        }

        private Vector2 ClosestPointAABB2D(Vector2 point, Vector2 center, Vector2 size)
        {
            var half = size * 0.5f;

            return new Vector2(
                Mathf.Clamp(point.x, center.x - half.x, center.x + half.x),
                Mathf.Clamp(point.y, center.y - half.y, center.y + half.y)
            );
        }

        private AttackDirection GetAttackDirection(Vector2 closestA, Vector2 closestB)
        {
            var dir = closestB - closestA;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                return AttackDirection.Side;
            }
            else
            {
                return dir.y > 0 ? AttackDirection.Up : AttackDirection.Down;
            }
        }
    }
}