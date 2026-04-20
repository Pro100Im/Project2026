using Code.Infrastructure.View.Registrars;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Spawn.Registrars
{
    public class PlayerSpawnPointsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Vector2Int[] _spawnPos;
        [Space]
        [SerializeField] private Tilemap _tilemap;

        public override void RegisterComponents()
        {
            var spawns = new List<Vector3>();

            foreach (var spawn in _spawnPos)
                spawns.Add(new Vector3(spawn.x, spawn.y));

            Entity.AddSpawnMap(spawns);
            Entity.isPlayer = true;
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveAllComponents();
        }

        private void OnDrawGizmosSelected()
        {
            if (_tilemap != null)
            {
                for (var i = 0; i < _spawnPos.Length; i++)
                {
                    var pos = _spawnPos[i];
                    var cell = _tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(cell, _tilemap.cellSize);
                }                
            }
        }
    }
}