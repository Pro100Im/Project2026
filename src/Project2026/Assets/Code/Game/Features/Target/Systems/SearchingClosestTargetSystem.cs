using Entitas;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    // To do
    public class SearchingClosestTargetSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _targets;
        private readonly IGroup<GameEntity> _enemies;

        public SearchingClosestTargetSystem(GameContext gameContext)
        {
            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Targetable,
                GameMatcher.Enemy));

            _targets = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Targetable,
                GameMatcher.Player));
        }

        public void Execute()
        {
            foreach (var enemy in _enemies)
            {
                var minDist = float.MaxValue;
                var nearestTargetId = -1;

                foreach (var warrior in _targets)
                {
                    if (warrior.isDead)
                        continue;

                    var dist = Vector3.Distance(enemy.transform.Value.position, warrior.transform.Value.position);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestTargetId = warrior.id.Value;
                    }
                }

                if (nearestTargetId != -1)
                {
                    if (!enemy.hasTargetId)
                        enemy.AddTargetId(nearestTargetId);
                    else if(enemy.targetId.Value != nearestTargetId)
                        enemy.ReplaceTargetId(nearestTargetId);
                }
                else
                {
                    if (enemy.hasTargetId)
                        enemy.RemoveTargetId();
                }
            }
        }
    }
}