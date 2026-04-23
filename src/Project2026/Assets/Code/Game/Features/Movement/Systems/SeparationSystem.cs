using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class SeparationSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;

        public SeparationSystem(GameContext ctx)
        {
            _units = ctx.GetGroup(GameMatcher.Transform);
        }

        public void Execute()
        {
            var units = _units.GetEntities();

            for (int i = 0; i < units.Length; i++)
            {
                var a = units[i];
                var aPos = a.transform.Value.position;
                var push = Vector3.zero;

                for (int j = 0; j < units.Length; j++)
                {
                    if (i == j) continue;

                    var b = units[j];
                    var diff = aPos - b.transform.Value.position;
                    var dist = diff.sqrMagnitude;

                    if (dist < 0.25f && dist > 0.0001f)
                    {
                        push += diff / dist;
                    }
                }

                a.ReplaceSeparationForce(push);
            }
        }
    }
}