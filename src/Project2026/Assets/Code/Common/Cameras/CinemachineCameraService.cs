using Unity.Cinemachine;
using UnityEngine;

namespace Code.Common.Cameras
{
    public class CinemachineCameraService : MonoBehaviour, ICameraService
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        public Camera GetCamera() => _camera;      
    }
}