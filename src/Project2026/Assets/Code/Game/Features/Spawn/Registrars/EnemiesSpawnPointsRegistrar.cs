using Code.Infrastructure.View.Registrars;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Spawn.Registrars
{
    public class EnemiesSpawnPointsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private SpawnPosition[] _enemiesSpawnPos;
        [Space]
        [SerializeField] private Tilemap _tilemap;

        public override void RegisterComponents()
        {
            var positions = new Vector3[_enemiesSpawnPos.Length];

            for (var i = 0; i < _enemiesSpawnPos.Length; i++)
            {
                var position = _tilemap.GetCellCenterWorld(_enemiesSpawnPos[i].Position);

                positions[i] = position;
            }

            Entity.AddSpawnPositions(positions);
            Entity.isEnemy = true;
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveAllComponents();
        }

        private void OnDrawGizmosSelected()
        {
            if (_tilemap != null)
            {
                for (var i = 0; i < _enemiesSpawnPos.Length; i++)
                {
                    var pos = _tilemap.GetCellCenterWorld(_enemiesSpawnPos[i].Position);

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(pos, _tilemap.cellSize);
                }
            }
        }

        [Serializable]
        private class SpawnPosition
        {
            [field: SerializeField] public int SortOrder { get; private set; }
            [field: SerializeField] public Vector3Int Position { get; private set; }
        }
    }
}