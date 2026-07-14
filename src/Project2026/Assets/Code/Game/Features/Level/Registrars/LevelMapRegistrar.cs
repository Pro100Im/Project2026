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
        [SerializeField] private Tilemap _flowTargets;
        [SerializeField] private Tilemap _defenseTargets;

        public override void RegisterComponents()
        {
            var dictionary = new Dictionary<Vector3Int, Vector3>();
            var bounds = _tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                var tile = _tilemap.GetTile(pos);

                if (tile == null)
                    continue;

                var worldPos = _tilemap.GetCellCenterWorld(pos);

                dictionary[pos] = worldPos;
            }

            var flowTargets = CollectTileCells(_flowTargets);
            var defenseTargets = CollectTileCells(_defenseTargets);

            Entity.AddTilemapMovement(dictionary);
            Entity.AddFlowFields(new());
            Entity.AddTargetFlow(flowTargets);
            Entity.AddIntegrationFields(new());
            Entity.AddDefenseFlowFields(new());
            Entity.AddDefenseFlow(defenseTargets);
            Entity.AddDefenseIntegrationFields(new());
            Entity.AddOccupField(new());
            Entity.AddReservedField(new());
            Entity.AddSpawnReservedField(new());
            Entity.AddSpatialHash(new());
            Entity.AddSurroundField(new());
            Entity.isFlowFieldDirty = true;
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveTilemapMovement();
            Entity.RemoveFlowFields();
            Entity.RemoveTargetFlow();
            Entity.RemoveIntegrationFields();
            Entity.RemoveDefenseFlowFields();
            Entity.RemoveDefenseFlow();
            Entity.RemoveDefenseIntegrationFields();
            Entity.RemoveOccupField();
            Entity.RemoveReservedField();
            Entity.RemoveSpawnReservedField();
            Entity.RemoveSpatialHash();
            Entity.RemoveSurroundField();
            Entity.isFlowFieldDirty = false;
        }

        private static List<Vector3Int> CollectTileCells(Tilemap tilemap)
        {
            var cells = new List<Vector3Int>();

            if (tilemap == null)
                return cells;

            var bounds = tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.GetTile(pos) == null)
                    continue;

                cells.Add(pos);
            }

            return cells;
        }

        private void OnDrawGizmosSelected()
        {
            DrawTargetGizmos(_flowTargets, Color.red);
            DrawTargetGizmos(_defenseTargets, Color.blue);
        }

        private static void DrawTargetGizmos(Tilemap tilemap, Color color)
        {
            if (tilemap == null)
                return;

            Gizmos.color = color;
            var bounds = tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.GetTile(pos) == null)
                    continue;

                var cell = tilemap.GetCellCenterWorld(pos);
                Gizmos.DrawWireCube(cell, tilemap.cellSize);
            }
        }
    }
}
