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

        public UpdateSpatialHashSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _targetables = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.CurrentCell,
                    GameMatcher.Targetable,
                    GameMatcher.Id)
                .NoneOf(GameMatcher.Dead));

            _maps = gameContext.GetGroup(GameMatcher.SpatialHash);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var hash = map.spatialHash.Value;

            foreach (var list in hash.Values)
                list.Clear();

            var targetables = _targetables.GetEntities(_buffer);

            for (var i = 0; i < targetables.Count; i++)
            {
                var target = targetables[i];
                var pos = target.currentCell.Value;
                var size = target.hasUnitSize ? target.unitSize.Value : Vector2Int.one;
                var id = target.id.Value;

                AddUnitToHash(hash, pos, size, id);

                if (target.hasTargetCell && !target.isDead)
                    AddUnitToHash(hash, target.targetCell.Value, size, id);
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