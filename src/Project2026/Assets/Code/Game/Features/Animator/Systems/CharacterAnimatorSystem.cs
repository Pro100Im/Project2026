using Entitas;

namespace Code.Game.Features.Animator.Systems
{
    public class CharacterAnimatorSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _characters;

        public CharacterAnimatorSystem(GameContext gameContext)
        {
            _characters = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.View,
                GameMatcher.Animator));
        }

        public void Execute()
        {
            foreach (var character in _characters)
            {
                var animator = character.animator.Value;
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                if (character.isMoving)
                {
                    if (!stateInfo.IsName("Run"))
                        animator.Play("Run");

                    continue;
                }

                if(character.isAttacking)
                {

                    continue;
                }

                if(character.isHitted)
                {

                    continue;
                }

                //if(character.isDead)
                //{
                //    continue;
                //}

                if (!stateInfo.IsName("Idle"))
                    animator.Play("Idle");
            }
        }
    }
}