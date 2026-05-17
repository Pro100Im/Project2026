using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level.Systems
{
    public class BuildFlowFieldSystem : ReactiveSystem<GameEntity>
    {
        private readonly TargetService _targetService;

        public BuildFlowFieldSystem(GameContext gameContext, TargetService targetService)
            : base(gameContext)
        {
            _targetService = targetService;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.FlowFieldDirty);
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasTilemapMovement && entity.isFlowFieldDirty;
        }

        protected override void Execute(List<GameEntity> maps)
        {
            for (int i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                var tilemap = map.tilemapMovement.Value;
                var occupField = map.occupField.Value;
                var goals = map.targetFlow.Value;
                var goalsSet = new HashSet<Vector3Int>(goals);

                var sizesToGenerate = new List<Vector2Int>
                {
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 2),
                    new Vector2Int(3, 3)
                };

                var allIntegrations = map.integrationFields.Value;
                var allFlows = map.flowFields.Value;

                foreach (var size in sizesToGenerate)
                {
                    if (!allIntegrations.ContainsKey(size)) 
                        allIntegrations[size] = new Dictionary<Vector3Int, int>();

                    if (!allFlows.ContainsKey(size)) 
                        allFlows[size] = new Dictionary<Vector3Int, Vector3Int>();

                    var integration = allIntegrations[size];
                    var flow = allFlows[size];

                    integration.Clear();
                    flow.Clear();

                    GenerateForSize(size, goals, goalsSet, tilemap, occupField, integration, flow);
                }

                map.isFlowFieldDirty = false;
            }
        }

        private void GenerateForSize(Vector2Int size, List<Vector3Int> goals, HashSet<Vector3Int> goalsSet, Dictionary<Vector3Int, Vector3> tilemap, 
            Dictionary<Vector3Int, int> occupField, Dictionary<Vector3Int, int> integration, Dictionary<Vector3Int, Vector3Int> flow)
        {
            var queue = new Queue<Vector3Int>();

            for (var i = 0; i < goals.Count; i++)
            {
                var goal = goals[i];
                for (int x = -(size.x - 1); x <= 0; x++)
                {
                    for (int y = -(size.y - 1); y <= 0; y++)
                    {
                        var anchorPos = new Vector3Int(goal.x + x, goal.y + y);

                        if (integration.ContainsKey(anchorPos)) 
                            continue;

                        if (CanFit(anchorPos, size, tilemap, occupField, goalsSet))
                        {
                            integration[anchorPos] = 0;
                            queue.Enqueue(anchorPos);
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var neighbors = _targetService.GetNeighbors(current);

                for (var i = 0; i < neighbors.Count; i++)
                {
                    var n = neighbors[i];
 
                    if (integration.ContainsKey(n)) 
                        continue;

                    if (!CanFit(n, size, tilemap, occupField, goalsSet))
                        continue;

                    if (IsCuttingCorner(current, n, size, tilemap, occupField, goalsSet))
                        continue;

                    var stepCost = GetStepCost(current, n);
                    var newCost = integration[current] + stepCost;

                    integration[n] = newCost;
                    queue.Enqueue(n);
                }
            }

            foreach (var cell in integration.Keys)
            {
                var best = cell;
                var bestCost = integration[cell];

                foreach (var n in _targetService.GetNeighbors(cell))
                {
                    if (integration.TryGetValue(n, out int nCost))
                    {
                        if (nCost < bestCost)
                        {
                            bestCost = nCost;
                            best = n;
                        }
                    }
                }

                flow[cell] = (best == cell) ? Vector3Int.zero : (best - cell);
            }
        }

        private bool CanFit(Vector3Int origin, Vector2Int size, Dictionary<Vector3Int, Vector3> tilemap, 
            Dictionary<Vector3Int, int> occupField, HashSet<Vector3Int> goalsSet)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var checkPos = new Vector3Int(origin.x + x, origin.y + y, 0);

                    if (!tilemap.ContainsKey(checkPos)) 
                        return false;

                    if (occupField.ContainsKey(checkPos))
                    {
                        if (goalsSet.Contains(checkPos))
                            continue;

                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsCuttingCorner(Vector3Int current, Vector3Int neighbor, Vector2Int size, Dictionary<Vector3Int, Vector3> tilemap, 
            Dictionary<Vector3Int, int> occupField, HashSet<Vector3Int> goalsSet)
        {
            if (current.x != neighbor.x && current.y != neighbor.y)
            {
                var corner1 = new Vector3Int(neighbor.x, current.y, 0);
                var corner2 = new Vector3Int(current.x, neighbor.y, 0);

                if (!CanFit(corner1, size, tilemap, occupField, goalsSet) || !CanFit(corner2, size, tilemap, occupField, goalsSet))
                    return true;
            }

            return false;
        }

        private int GetStepCost(Vector3Int a, Vector3Int b)
        {
            return (a.x != b.x && a.y != b.y) ? 14 : 10;
        }
    }
}