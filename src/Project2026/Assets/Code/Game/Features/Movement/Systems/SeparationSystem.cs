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

                Vector3 push = Vector3.zero;

                for (int j = 0; j < units.Length; j++)
                {
                    if (i == j) continue;

                    var b = units[j];
                    var diff = aPos - b.transform.Value.position;

                    float dist = diff.sqrMagnitude;

                    if (dist < 0.5f * 0.5f && dist > 0.0001f)
                    {
                        float inv = 1f - Mathf.Sqrt(dist) / 0.5f;
                        push += diff.normalized * inv;
                    }
                }

                a.ReplaceSeparationForce(push);
            }
        }
    }
}