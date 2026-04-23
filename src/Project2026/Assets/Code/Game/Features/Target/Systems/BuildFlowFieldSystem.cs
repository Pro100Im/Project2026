using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class BuildFlowFieldSystem : ReactiveSystem<GameEntity>
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _targets;

        public BuildFlowFieldSystem(GameContext gameContext, TargetService targetService)
            : base(gameContext)
        {
            _targetService = targetService;

            _targets = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Targetable,
                GameMatcher.PlayerCastle));
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
                    integration[goal] = 0;
                }


                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();

                    foreach (var n in _targetService.GetCardinalNeighbors(current))
                    {
                        if (!tilemap.ContainsKey(n))
                            continue;

                        int cost = integration[current] + 1;

                        if (!integration.ContainsKey(n) || cost < integration[n])
                        {
                            integration[n] = cost;
                            queue.Enqueue(n);
                        }
                    }
                }


                foreach (var cell in integration.Keys)
                {
                    Vector3Int best = cell;
                    int bestCost = integration[cell];

                    foreach (var n in _targetService.GetCardinalNeighbors(cell))
                    {
                        if (!integration.ContainsKey(n))
                            continue;

                        if (integration[n] < bestCost)
                        {
                            bestCost = integration[n];
                            best = n;
                        }
                    }

                    flow[cell] = (best == cell) ? Vector3Int.zero : (best - cell);
                }

                map.ReplaceFlowField(flow);
                map.isFlowFieldDirty = false;
            }
        }
    }
}