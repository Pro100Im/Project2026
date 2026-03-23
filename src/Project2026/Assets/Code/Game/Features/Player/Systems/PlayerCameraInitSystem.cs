using Code.Common.Cameras;
using Cysharp.Threading.Tasks;
using Entitas;
using Unity.Netcode;

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
            _cameraService.SetActiveCamera(NetworkManager.Singleton.LocalClientId);
        }
    }
}