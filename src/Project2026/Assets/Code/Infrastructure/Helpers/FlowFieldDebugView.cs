using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR

namespace Code.Infrastructure.Helpers
{
    public class FlowFieldDebugView : MonoBehaviour
    {
        [Header("Настройки Tilemap")]
        [SerializeField] private Tilemap _tilemap;

        [Header("Выбор размера для отладки")]
        [Tooltip("Введите размер юнита (напр. 1x1, 2x2), чтобы увидеть его поле")]
        [SerializeField] private Vector2Int _debugSize = new Vector2Int(1, 1);

        [Header("Визуализация")]
        [SerializeField] private bool _showVectors = true;
        [SerializeField] private bool _showCosts = true;
        [SerializeField] private float _vectorLength = 0.4f;

        private void OnDrawGizmos()
        {
            // Не рисуем, если нет тайлмапа или игра не запущена
            if (_tilemap == null || !Application.isPlaying)
                return;

            var gameContext = Contexts.sharedInstance.game;

            // ВАЖНО: Теперь ищем сущность с множественными полями (FlowFields / IntegrationFields)
            var mapEntity = gameContext.GetGroup(GameMatcher.AllOf(
                GameMatcher.FlowFields,
                GameMatcher.IntegrationFields)).GetSingleEntity();

            if (mapEntity == null)
                return;

            // 1. Проверяем, есть ли расчеты для выбранного размера
            if (!mapEntity.integrationFields.Value.TryGetValue(_debugSize, out var integration))
                return;

            // Пытаемся достать векторы (они могут отсутствовать, если BFS не нашел путь)
            mapEntity.flowFields.Value.TryGetValue(_debugSize, out var flow);

            foreach (var kvp in integration)
            {
                Vector3Int cellPos = kvp.Key;
                int cost = kvp.Value;
                Vector3 worldPos = _tilemap.GetCellCenterWorld(cellPos);

                // 2. Рисуем стоимость (Integration Field)
                if (_showCosts)
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = GetColorForCost(cost);
                    style.fontSize = 10;
                    style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
                    UnityEditor.Handles.Label(worldPos + Vector3.up * 0.2f, cost.ToString(), style);
#endif
                }

                // 3. Рисуем векторы (Flow Field)
                if (_showVectors && flow != null && flow.TryGetValue(cellPos, out Vector3Int direction))
                {
                    if (direction != Vector3Int.zero)
                    {
                        Gizmos.color = Color.cyan;
                        Vector3 targetWorldPos = _tilemap.GetCellCenterWorld(cellPos + direction);
                        DrawArrow(worldPos, (targetWorldPos - worldPos).normalized * _vectorLength);
                    }
                    else if (cost == 0) // Если стоимость 0 и вектора нет — это финиш (замок)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(worldPos, Vector3.one * 0.3f);
                    }
                }
            }
        }

        private Color GetColorForCost(int cost)
        {
            if (cost >= 1000) return Color.red; // Непроходимо для данного размера
            if (cost == 0) return Color.green;   // Цель
            return Color.white;
        }

        private void DrawArrow(Vector3 pos, Vector3 direction)
        {
            Gizmos.DrawRay(pos, direction);

            // Рисуем наконечник
            if (direction.sqrMagnitude > 0.01f)
            {
                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * Vector3.forward;
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * Vector3.forward;
                Gizmos.DrawRay(pos + direction, right * 0.1f);
                Gizmos.DrawRay(pos + direction, left * 0.1f);
            }
        }
    }
}

#endif