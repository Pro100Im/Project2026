using Code.Infrastructure.View.Registrars;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Player.Registrars
{
    [Serializable]
    public class CurrentCellRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Tilemap _tilemap;
        [Space]
        [SerializeField] private Vector3Int _cell;
        [SerializeField] private Vector2Int _size;

        public override void RegisterComponents()
        {
            Entity.AddCurrentCell(_cell);
            Entity.AddUnitSize(_size);
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveCurrentCell();
            Entity.RemoveUnitSize();
        }

        private void OnDrawGizmosSelected()
        {
            if (_tilemap == null) 
                return;

            for (var x = 0; x < _size.x; x++)
            {
                for (var y = 0; y < _size.y; y++)
                {
                    var currentCellPos = new Vector3Int(_cell.x + x, _cell.y + y, _cell.z);
                    var worldPos = _tilemap.GetCellCenterWorld(currentCellPos);

                    Gizmos.color = Color.darkGreen;
                    Gizmos.DrawWireCube(worldPos, _tilemap.cellSize);
                }
            }

            var anchorPos = _tilemap.GetCellCenterWorld(_cell);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(anchorPos, 0.1f);
        }
    }
}
