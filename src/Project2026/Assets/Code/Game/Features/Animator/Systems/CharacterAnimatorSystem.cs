using Code.Game.Features.Attack;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Animator.Systems
{
    // to do
    public class CharacterAnimatorSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _characters;

        private readonly List<GameEntity> _charactersBuffer = new(512);

        public CharacterAnimatorSystem(GameContext gameContext)
        {
            _characters = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.View,
                GameMatcher.Animator,
                GameMatcher.Unit));
        }

        public void Execute()
        {
            var characters = _characters.GetEntities(_charactersBuffer);

            for (var i = 0; i < characters.Count; i++)
            {
                var character = characters[i];
                var animator = character.animator.Value;
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                if (character.isDead)
                {
                    if (!stateInfo.IsName("Dead"))
                        animator.Play("Dead");

                    continue;
                }

                if (character.isAttacking)
                {
                    if (!character.hasAttackDirection)
                        continue;

                    var attackState = GetAttackStateName(character.attackDirection.Value);

                    if (!character.isAttackAnimStarted)
                    {
                        if (!stateInfo.IsName(attackState))
                            animator.Play(attackState, 0, 0f);

                        character.isAttackAnimStarted = true;
                    }

                    continue;
                }

                if (character.isMoving)
                {
                    if (!stateInfo.IsName("Run"))
                        animator.Play("Run");

                    continue;
                }

                if (character.isHitted)
                {
                    continue;
                }

                if (!stateInfo.IsName("Idle"))
                    animator.Play("Idle");
            }
        }

        private static string GetAttackStateName(AttackDirection direction)
        {
            switch (direction)
            {
                case AttackDirection.Up:
                    return "AttackUp";
                case AttackDirection.Down:
                    return "AttackDown";
                default:
                    return "AttackRight";
            }
        }
    }
}
