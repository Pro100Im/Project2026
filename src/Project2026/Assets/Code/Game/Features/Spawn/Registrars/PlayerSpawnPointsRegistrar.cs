using Code.Infrastructure.View.Registrars;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Spawn.Registrars
{
    public class PlayerSpawnPointsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Vector3Int[] _playersSpawnPos;
        [Space]
        [SerializeField] private Tilemap _tilemap;

        public override void RegisterComponents()
        {
            var points = new Vector3[_playersSpawnPos.Length];

            for(var i = 0; i < _playersSpawnPos.Length; i++)
            {
                points[i] = _tilemap.GetCellCenterWorld(_playersSpawnPos[i]);
            }

            Entity.AddSpawnPositions(points);
            Entity.isForPlayer = true;
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveAllComponents();
        }

        private void OnDrawGizmosSelected()
        {
            if (_tilemap != null)
            {
                for (var i = 0; i < _playersSpawnPos.Length; i++)
                {
                    var pos = _tilemap.GetCellCenterWorld(_playersSpawnPos[i]);

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(pos, _tilemap.cellSize);
                }                
            }
        }
    }
}