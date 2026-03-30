using Code.Infrastructure.View.Registrars;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Movement.Registrars
{
    public class MovementPointsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] public int _gatennumber;
        [SerializeField] private Vector3Int[] _movementPoints;
        [Space]
        [SerializeField] private Tilemap _tilemap;
        [Space]
        [SerializeField] private Color _gizmoColor = Color.azure;

        public override void RegisterComponents()
        {
            var points = new Vector3[_movementPoints.Length];

            for (var i = 0; i < _movementPoints.Length; i++)
            {
                var position = _tilemap.GetCellCenterWorld(_movementPoints[i]);

                points[i] = position;
            }

            Entity.AddMovementPoints(points);
            Entity.AddGateNumber(_gatennumber);
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
                for (var i = 0; i < _movementPoints.Length; i++)
                {
                    var pos = _tilemap.GetCellCenterWorld(_movementPoints[i]);

                    Gizmos.color = _gizmoColor;
                    Gizmos.DrawWireCube(pos, _tilemap.cellSize);
                }
            }
        }
    }
}