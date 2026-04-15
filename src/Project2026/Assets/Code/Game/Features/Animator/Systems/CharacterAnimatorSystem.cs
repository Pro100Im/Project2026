using Code.Game.Features.Attack;
using Entitas;

namespace Code.Game.Features.Animator.Systems
{
    // to do
    public class CharacterAnimatorSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _characters;

        public CharacterAnimatorSystem(GameContext gameContext)
        {
            _characters = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.View,
                GameMatcher.Animator,
                GameMatcher.Enemy));
        }

        public void Execute()
        {
            foreach (var character in _characters)
            {
                var animator = character.animator.Value;
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                if (character.isDead)
                {
                    if (!stateInfo.IsName("Dead"))
                        animator.Play("Dead");

                    continue;
                }

                if (character.isMoving)
                {
                    if (!stateInfo.IsName("Run"))
                        animator.Play("Run");

                    continue;
                }

                if(character.isAttacking)
                {
                    switch (character.attackDirection.Value)
                    {
                        case AttackDirection.Side:
                            if (!stateInfo.IsName("AttackRight"))
                                animator.Play("AttackRight");
                            break;

                        case AttackDirection.Up:
                            if (!stateInfo.IsName("AttackUp"))
                                animator.Play("AttackUp");
                            break;

                        case AttackDirection.Down:
                            if (!stateInfo.IsName("AttackDown"))
                                animator.Play("AttackDown");
                            break;
                    }

                    continue;
                }

                if(character.isHitted)
                {

                    continue;
                }

                if (!stateInfo.IsName("Idle"))
                    animator.Play("Idle");
            }
        }
    }
}