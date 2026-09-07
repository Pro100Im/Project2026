using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Animator.Systems
{
    public class CannonAnimatorSystem : IExecuteSystem
    {
        private static readonly int DirectionHash = UnityEngine.Animator.StringToHash("Direction");
        private static readonly int AttackingHash = UnityEngine.Animator.StringToHash("Attacking");

        private readonly IGroup<GameEntity> _cannons;
        private readonly List<GameEntity> _buffer = new(64);

        public CannonAnimatorSystem()
        {
            _cannons = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.CannonAnimator,
                    GameMatcher.CannonSpriteRenderer));
        }

        public void Execute()
        {
            var cannons = _cannons.GetEntities(_buffer);

            for (var i = 0; i < cannons.Count; i++)
            {
                var entity = cannons[i];
                var spriteRenderer = entity.cannonSpriteRenderer.Value;
                var animator = entity.cannonAnimator.Value;

                if (!entity.isTower)
                {
                    if (spriteRenderer.enabled)
                        spriteRenderer.enabled = false;

                    continue;
                }

                if (!spriteRenderer.enabled)
                    spriteRenderer.enabled = true;

                if (entity.hasCannonAimDirection)
                    animator.SetInteger(DirectionHash, (int)entity.cannonAimDirection.Value);

                animator.SetBool(AttackingHash, entity.isAttacking);

                var shouldFlipX = entity.isCannonFlipX;

                if (spriteRenderer.flipX != shouldFlipX)
                    spriteRenderer.flipX = shouldFlipX;
            }
        }
    }
}
