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
            var dictionary = new Dictionary<Vector3, bool>();
            var bounds = _tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                var tile = _tilemap.GetTile(pos);
                var walkable = tile != null;
                var worldPos = _tilemap.GetCellCenterWorld(pos);

                dictionary[worldPos] = walkable;
            }

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