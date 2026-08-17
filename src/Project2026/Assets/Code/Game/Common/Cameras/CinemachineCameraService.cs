using Unity.Cinemachine;
using UnityEngine;

namespace Code.Game.Common.Cameras
{
    public class CinemachineCameraService : MonoBehaviour, ICameraService
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        public Camera GetCamera() => _camera;

        public void SetActiveMainCamera()
        {
            _cinemachineCamera.Priority = 1;
        }

        public void SetActiveTownCamera()
        {
            _cinemachineCamera.Priority = -1;
        }
    }
}