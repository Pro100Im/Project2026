using Code.Common.Cameras;
using Entitas;

namespace Code.Game.Features.Player.Systems
{
    public class PlayerCameraInitSystem : IInitializeSystem
    {
        private readonly ICameraService _cameraService;

        public PlayerCameraInitSystem(GameContext game, ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public void Initialize()
        {
            
        }
    }
}