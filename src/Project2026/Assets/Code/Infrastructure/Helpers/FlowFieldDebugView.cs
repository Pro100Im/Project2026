using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR

namespace Code.Infrastructure.Helpers
{
    public class FlowFieldDebugView : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [Space]
        [SerializeField] private bool _showVectors = true;
        [SerializeField] private bool _showCosts = true;
        [SerializeField] private float _vectorLength = 0.4f;

        public Dictionary<Vector3Int, Vector3Int> CurrentFlowField;
        public Dictionary<Vector3Int, int> CurrentIntegrationField;

        private void OnDrawGizmos()
        {
            if (_tilemap == null) 
                return;

            if (!Application.isPlaying) 
                return;

            var gameContext = Contexts.sharedInstance.game;
            var mapEntity = gameContext.GetGroup(GameMatcher.AllOf(GameMatcher.FlowField, GameMatcher.IntegrationField)).GetSingleEntity();

            if (mapEntity == null) 
                return;

            var flow = mapEntity.flowField.Value;
            var integration = mapEntity.integrationField.Value;

            foreach (var kvp in integration)
            {
                Vector3Int cellPos = kvp.Key;
                int cost = kvp.Value;
                Vector3 worldPos = _tilemap.GetCellCenterWorld(cellPos);

                if (_showCosts)
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = GetColorForCost(cost);
                    style.fontSize = 10;

                    UnityEditor.Handles.Label(worldPos + Vector3.up * 0.2f, cost.ToString(), style);
                }

                if (_showVectors && flow.TryGetValue(cellPos, out Vector3Int direction))
                {
                    if (direction != Vector3Int.zero)
                    {
                        Gizmos.color = Color.cyan;
                        Vector3 targetWorldPos = _tilemap.GetCellCenterWorld(cellPos + direction);
                        DrawArrow(worldPos, (targetWorldPos - worldPos).normalized * _vectorLength);
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(worldPos, 0.05f);
                    }
                }
            }
        }

        private Color GetColorForCost(int cost)
        {
            if (cost >= 255) return Color.red;
            if (cost == 0) return Color.green;

            return Color.white;
        }

        private void DrawArrow(Vector3 pos, Vector3 direction)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * Vector3.forward;

            Gizmos.DrawRay(pos + direction, right * 0.1f);
            Gizmos.DrawRay(pos + direction, left * 0.1f);
        }
    }
}

#endif