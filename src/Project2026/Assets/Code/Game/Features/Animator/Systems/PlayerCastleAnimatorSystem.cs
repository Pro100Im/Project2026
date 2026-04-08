using Entitas;

namespace Code.Game.Features.Animator.Systems
{
    public class PlayerCastleAnimatorSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _castles;

        public PlayerCastleAnimatorSystem(GameContext gameContext)
        {
            _castles = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.View,
                    GameMatcher.Animator,
                    GameMatcher.PlayerCastle));
        }

        public void Execute()
        {
            foreach (var castle in _castles)
            {
                var animator = castle.animator.Value;
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                if (castle.isDead && !stateInfo.IsName("CastleDestroy"))
                    animator.Play("CastleDestroy");
            }
        }
    }
}