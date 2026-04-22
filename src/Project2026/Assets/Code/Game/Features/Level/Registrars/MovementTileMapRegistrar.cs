using Code.Infrastructure.View.Registrars;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Level.Registrars
{
    public class MovementTileMapRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Tilemap _tilemap;

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

            Entity.AddGridSize(_tilemap.cellSize);
            Entity.AddTilemapMovement(dictionary);
            Entity.AddOccupancyMap(new());
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveTilemapMovement();
            Entity.RemoveOccupancyMap();
        }
    }
}