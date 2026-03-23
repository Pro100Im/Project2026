using Entitas;
using Unity.Netcode;
using UnityEngine;

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
                GameMatcher.ClientId,
                GameMatcher.PlayerAnimator,
                GameMatcher.CurrentSpeed,
                GameMatcher.MaxRunSpeed));
        }

        public void Execute()
        {
            foreach (var player in _players)
            {
                if (player.clientId.Value != NetworkManager.Singleton.LocalClientId)
                    continue;

                var dir = player.direction.Value * (player.currentSpeed.Value / player.maxRunSpeed.Value);

                var normalizedSpeed = dir.normalized;

                player.playerAnimator.Value.SetMoveParameters(normalizedSpeed.x, normalizedSpeed.z);
            }
        }
    }
}
