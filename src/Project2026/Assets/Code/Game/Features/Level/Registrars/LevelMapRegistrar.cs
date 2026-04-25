using Code.Infrastructure.View.Registrars;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Level.Registrars
{
    public class LevelMapRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Tilemap _tilemap;
        [Space]
        [SerializeField] private Vector3Int[] _flowTargets;

        public override void RegisterComponents()
        {
            var dictionary = new Dictionary<Vector3Int, Vector3>();
            var flowTargets = new List<Vector3Int>();
            var bounds = _tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                var tile = _tilemap.GetTile(pos);

                if (tile == null)
                    continue;

                var worldPos = _tilemap.GetCellCenterWorld(pos);

                dictionary[pos] = worldPos;
            }

            foreach(var target in _flowTargets)
                flowTargets.Add(target);

            Entity.AddTilemapMovement(dictionary);
            Entity.AddFlowField(new());
            Entity.AddTargetFlow(flowTargets);
            Entity.AddIntegrationField(new());
            Entity.isFlowFieldDirty = true;
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveTilemapMovement();
            Entity.RemoveFlowField();
            Entity.RemoveTargetFlow();
            Entity.RemoveIntegrationField();
            Entity.isFlowFieldDirty = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (_tilemap != null && _flowTargets != null)
            {
                for (var i = 0; i < _flowTargets.Length; i++)
                {
                    var pos = _flowTargets[i];
                    var cell = _tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(cell, _tilemap.cellSize);
                }
            }
        }
    }
}