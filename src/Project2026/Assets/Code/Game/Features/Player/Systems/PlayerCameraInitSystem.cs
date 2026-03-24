using Code.Common.Cameras;
using Entitas;
using Unity.Netcode;

namespace Code.Game.Features.Player.Systems
{
    public class PlayerCameraInitSystem : IInitializeSystem
    {
        private readonly IGroup<GameEntity> _players;
        private readonly ICameraService _cameraService;

        public PlayerCameraInitSystem(GameContext game, ICameraService cameraService)
        {
            _players = game.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Player,
                GameMatcher.ClientId,
                GameMatcher.PlayerAreaNumber));

            _cameraService = cameraService;
        }

        public void Initialize()
        {
            foreach(var player in _players)
            {
                if(player.clientId.Value == NetworkManager.Singleton.LocalClientId)
                    _cameraService.SetActiveCamera(player.playerAreaNumber.Value);
            }
        }
    }
}