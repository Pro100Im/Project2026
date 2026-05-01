using Code.Game.Features.Level;
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
            return entity.hasTilemapMovement;
        }

        protected override void Execute(List<GameEntity> maps)
        {
            foreach (var map in maps)
            {
                var tilemap = map.tilemapMovement.Value;
                var integration = new Dictionary<Vector3Int, int>();
                var flow = new Dictionary<Vector3Int, Vector3Int>();
                var queue = new Queue<Vector3Int>();

                foreach (var goal in map.targetFlow.Value)
                {
                    if (!tilemap.ContainsKey(goal)) 
                        continue;

                    queue.Enqueue(goal);
                    integration[goal] = map.occupField.Value.ContainsKey(goal) ? 100 : 0;
                }

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();

                    foreach (var n in _targetService.GetNeighbors(current))
                    {
                        if (!tilemap.ContainsKey(n)) 
                            continue;

                        var stepCost = GetStepCost(current, n);
                        var newCost = integration[current] + stepCost;

                        if (!integration.ContainsKey(n) || newCost < integration[n])
                        {
                            if (IsCuttingCorner(current, n, tilemap))
                                continue;

                            integration[n] = newCost;
                            queue.Enqueue(n);
                        }
                    }
                }

                foreach (var cell in integration.Keys)
                {
                    var best = cell;
                    var bestCost = integration[cell];

                    foreach (var n in _targetService.GetNeighbors(cell))
                    {
                        if (!integration.ContainsKey(n)) continue;

                        if (integration[n] < bestCost)
                        {
                            bestCost = integration[n];
                            best = n;
                        }
                    }

                    flow[cell] = (best == cell) ? Vector3Int.zero : (best - cell);
                }

                map.ReplaceFlowField(flow);
                map.ReplaceIntegrationField(integration);
                map.isFlowFieldDirty = false;
            }
        }

        private int GetStepCost(Vector3Int a, Vector3Int b)
        {
            return (a.x != b.x && a.y != b.y) ? 14 : 10;
        }

        private bool IsCuttingCorner(Vector3Int current, Vector3Int neighbor, Dictionary<Vector3Int, Vector3> tilemap)
        {
            if (current.x != neighbor.x && current.y != neighbor.y)
            {
                var corner1 = new Vector3Int(neighbor.x, current.y, 0);
                var corner2 = new Vector3Int(current.x, neighbor.y, 0);

                if (!tilemap.ContainsKey(corner1) || !tilemap.ContainsKey(corner2))
                    return true;
            }

            return false;
        }
    }
}