using Entitas;

namespace Code.Game.Features.Player.Systems
{
    public class PlayerAnimatorSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _players;

        public PlayerAnimatorSystem(GameContext gameContext)
        {
            _players = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Player,
                GameMatcher.Player,
                GameMatcher.PlayerAnimator,
                GameMatcher.CurrentSpeed,
                GameMatcher.MaxRunSpeed));
        }

        public void Execute()
        {
            foreach (var player in _players)
            {
                var dir = player.direction.Value * (player.currentSpeed.Value / player.maxRunSpeed.Value);

                var normalizedSpeed = dir.normalized;

                player.playerAnimator.Value.SetMoveParameters(normalizedSpeed.x, normalizedSpeed.z);
            }
        }
    }
}
