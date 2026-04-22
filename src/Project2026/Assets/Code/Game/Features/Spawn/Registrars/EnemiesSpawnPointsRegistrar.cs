using Code.Infrastructure.View.Registrars;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Spawn.Registrars
{
    public class EnemiesSpawnPointsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Tilemap _tilemap;

        public override void RegisterComponents()
        {
            var spawns = new Dictionary<Vector3Int, Vector3>();
            var bounds = _tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                var tile = _tilemap.GetTile(pos);

                if (tile == null)
                    continue;

                var worldPos = _tilemap.GetCellCenterWorld(pos);

                spawns[pos] = worldPos;
            }

            Entity.AddSpawnMap(spawns);
            Entity.isEnemy = true;
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveAllComponents();
        }
    }
}