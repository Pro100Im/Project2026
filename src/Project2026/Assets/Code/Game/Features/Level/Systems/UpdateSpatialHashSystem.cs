using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level.Systems
{
    public class UpdateSpatialHashSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _targetables;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(512);

        public UpdateSpatialHashSystem(GameContext context)
        {
            _targetables = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.CurrentCell,
                    GameMatcher.Targetable,
                    GameMatcher.Id)
                .NoneOf(GameMatcher.Dead));

            _maps = context.GetGroup(GameMatcher.SpatialHash);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var hash = map.spatialHash.Value;

            foreach (var list in hash.Values)
                list.Clear();

            foreach (var e in _targetables.GetEntities(_buffer))
            {
                var pos = e.currentCell.Value;
                var size = e.hasUnitSize ? e.unitSize.Value : Vector2Int.one;
                var id = e.id.Value;

                AddUnitToHash(hash, pos, size, id);

                if (e.hasTargetCell && !e.isDead)
                    AddUnitToHash(hash, e.targetCell.Value, size, id);
            }
        }

        private void AddUnitToHash(Dictionary<Vector2Int, List<int>> hash, Vector3Int origin, Vector2Int size, int id)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var cell = new Vector2Int(origin.x + x, origin.y + y);

                    if (!hash.TryGetValue(cell, out var list))
                    {
                        list = new List<int>(4);

                        hash[cell] = list;
                    }

                    list.Add(id);
                }
            }
        }
    }
}