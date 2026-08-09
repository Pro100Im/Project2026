using Code.Game.Common.Cameras;
using Entitas;

namespace Code.Game.Features.Player.Systems
{
    public class PlayerCameraInitSystem : IInitializeSystem
    {
        private readonly ICameraService _cameraService;

        public PlayerCameraInitSystem(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public void Initialize()
        {
            
        }
    }
}