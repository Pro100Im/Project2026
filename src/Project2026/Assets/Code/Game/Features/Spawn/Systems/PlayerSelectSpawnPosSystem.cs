using Code.Game.Common.Random;
using Code.Game.Features.Level;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Spawn.Systems
{
    public class PlayerSelectSpawnPosSystem : IExecuteSystem
    {
        private readonly IRandomService _random;
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _spawnMaps;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _spawnMapsBuffer = new(8);
        private readonly List<GameEntity> _unitsBuffer = new(LevelBuffers.DefaultCapacity);

        public PlayerSelectSpawnPosSystem(IRandomService random)
        {
            var gameContext = Contexts.sharedInstance.game;

            _random = random;

            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpawnRequsted,
                    GameMatcher.Player,
                    GameMatcher.UnitSize));

            _spawnMaps = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpawnMap,
                    GameMatcher.Player));

            _maps = gameContext.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;
            var spawnMaps = _spawnMaps.GetEntities(_spawnMapsBuffer);

            // Shares this frame's reservations with the enemy spawns instead of wiping them.
            arena.BeginSpawnFrame(Time.frameCount);
            LevelBuffers.Fit(_unitsBuffer, arena);

            for (var i = 0; i < spawnMaps.Count; i++)
            {
                var spawnMap = spawnMaps[i];
                var points = spawnMap.spawnMap.Value;
                var units = _units.GetEntities(_unitsBuffer);

                for (var j = 0; j < units.Count; j++)
                {
                    var unit = units[j];

                    if (unit.hasSpawnPosition)
                        continue;

                    var size = unit.unitSize.Value;
                    var count = 0;
                    var chosenPos = Vector3.zero;
                    var chosenCell = Vector3Int.zero;
                    var found = false;

                    foreach (var kvp in points)
                    {
                        var cell = kvp.Key;
                        var worldPos = kvp.Value;

                        if (!SpawnCells.CanFit(arena, cell, size))
                            continue;

                        count++;

                        if (_random.GetGlobalRandom(0, count) == 0)
                        {
                            chosenPos = worldPos;
                            chosenCell = cell;
                            found = true;
                        }
                    }

                    if (found)
                    {
                        unit.AddSpawnPosition(chosenPos);
                        unit.AddCurrentCell(chosenCell);

                        SpawnCells.Reserve(arena, chosenCell, size);
                    }
                }
            }
        }
    }
}
