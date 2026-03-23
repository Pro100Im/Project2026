using Code.Infrastructure.View.Registrars;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Spawn.Registrars
{
    public class PlayerSpawnPointsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Vector3Int[] _spawnPos;
        [SerializeField] private Tilemap _tilemap;

        private Vector3[] _points;

        public override void RegisterComponents()
        {
            _points = new Vector3[_spawnPos.Length];

            for(var i = 0; i < _spawnPos.Length; i++)
            {
                _points[i] = _tilemap.GetCellCenterWorld(_spawnPos[i]);
            }

            Entity.AddSpawnPositions(_points);
            Entity.isForPlayer = true;
            Entity.isFreePoint = true;
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
                    var pos = _tilemap.GetCellCenterWorld(_spawnPos[i]);

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(pos, _tilemap.cellSize);
                }                
            }
        }
    }
}