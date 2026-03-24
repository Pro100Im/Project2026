using Code.Infrastructure.View.Registrars;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Spawn.Registrars
{
    public class EnemiesSpawnPointsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private int _areaNumber;
        [SerializeField] private Vector3Int[] _enemiesSpawnPos;
        [Space]
        [SerializeField] private Tilemap _tilemap;

        public override void RegisterComponents()
        {
            var positions = new Vector3[_enemiesSpawnPos.Length];

            for (var i = 0; i < _enemiesSpawnPos.Length; i++)
            {
                var position = _tilemap.GetCellCenterWorld(_enemiesSpawnPos[i]);

                positions[i] = position;
            }

            Entity.AddSpawnPositions(positions);
            Entity.AddPlayerAreaNumber(_areaNumber);
            Entity.isForPlayer = false;
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
                    var pos = _tilemap.GetCellCenterWorld(_enemiesSpawnPos[i]);

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(pos, _tilemap.cellSize);
                }
            }
        }
    }
}