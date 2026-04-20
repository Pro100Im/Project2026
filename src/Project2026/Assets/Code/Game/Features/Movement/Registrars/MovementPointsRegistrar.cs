using Code.Infrastructure.View.Registrars;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Movement.Registrars
{
    public class MovementPointsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] public int _gatennumber;
        [SerializeField] private MovementPath[] _movementPath;
        [Space]
        [SerializeField] private Tilemap _tilemap;
        [Space]
        [SerializeField] private Color _gizmoColor = Color.azure;

        public override void RegisterComponents()
        {
            var minDistances = new float[_movementPath.Length];
            var points = new Vector2[_movementPath.Length];
            var minOffsets = new Vector2[_movementPath.Length];
            var maxOffsets = new Vector2[_movementPath.Length];

            for (var i = 0; i < _movementPath.Length; i++)
            {
                var movementPoint = _movementPath[i];

                minDistances[i] = movementPoint.MinDistance;
                points[i] = _tilemap.GetCellCenterWorld(movementPoint.MovementPoint);
                minOffsets[i] = movementPoint.MinMovementOffset;
                maxOffsets[i] = movementPoint.MaxMovementOffset;
            }

            Entity.AddMovementPoints(points);
            Entity.AddMovementPointMinDistances(minDistances);
            Entity.AddMinMovementOffsets(minOffsets);
            Entity.AddMaxMovementOffsets(maxOffsets);
            Entity.isEnemy = true;
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveAllComponents();
        }

        private void OnDrawGizmosSelected()
        {
            if (_tilemap != null && _movementPath != null)
            {
                for (var i = 0; i < _movementPath.Length; i++)
                {
                    var pos = (Vector2)_tilemap.GetCellCenterWorld(_movementPath[i].MovementPoint);

                    Gizmos.color = _gizmoColor;
                    Gizmos.DrawWireCube(pos, _tilemap.cellSize);

                    var offsetMinPos = pos + _movementPath[i].MinMovementOffset;
                    var offsetMaxPos = pos + _movementPath[i].MaxMovementOffset;

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(offsetMinPos, 0.1f);

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(offsetMaxPos, 0.1f);

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(offsetMinPos, offsetMaxPos);
                }
            }
        }


        [Serializable]
        private class MovementPath
        {
            public float MinDistance;
            public Vector3Int MovementPoint;
            public Vector2 MinMovementOffset;
            public Vector2 MaxMovementOffset;
        }
    }
}