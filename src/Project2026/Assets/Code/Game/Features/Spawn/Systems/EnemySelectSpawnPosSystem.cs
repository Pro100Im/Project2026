using Code.Game.Common.Random;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySelectSpawnPosSystem : IExecuteSystem
    {
        private readonly IRandomService _random;

        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _spawnMaps;
        private readonly IGroup<GameEntity> _units;

        public EnemySelectSpawnPosSystem(GameContext context, IRandomService random)
        {
            _random = random;

            _enemies = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpawnRequsted,
                    GameMatcher.Enemy,
                    GameMatcher.UnitSize));

            _spawnMaps = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpawnMap,
                    GameMatcher.Enemy));

            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.UnitSize));
        }

        public void Execute()
        {
            foreach (var spawnMap in _spawnMaps)
            {
                var points = spawnMap.spawnMap.Value;

                foreach (var enemy in _enemies)
                {
                    if (enemy.hasSpawnPosition)
                        continue;

                    var radius = enemy.unitSize.Value;
                    var count = 0;
                    var chosenPos = Vector3.zero;
                    var chosenCell = Vector3Int.zero;

                    bool found = false;

                    foreach (var kvp in points)
                    {
                        var cell = kvp.Key;
                        var worldPos = kvp.Value;

                        if (!CanSpawnAt(worldPos, radius))
                            continue;

                        count++;

                        if (_random.GetGlobalRandom(0, count) == 0)
                        {
                            chosenPos = worldPos;
                            chosenCell = cell;
                            found = true;
                        }
                    }

                    if (!found)
                        continue;

                    enemy.AddSpawnPosition(chosenPos);
                    enemy.AddCurrentCell(chosenCell);
                }
            }
        }

        private bool CanSpawnAt(Vector3 pos, float radius)
        {
            foreach (var unit in _units)
            {
                var otherPos = unit.transform.Value.position;
                var otherRadius = unit.unitSize.Value;

                var dist = Vector3.Distance(pos, otherPos);
                var minDist = radius + otherRadius;

                if (dist < minDist)
                    return false;
            }

            return true;
        }
    }
}