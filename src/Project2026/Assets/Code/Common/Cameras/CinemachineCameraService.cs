using Unity.Cinemachine;
using UnityEngine;

namespace Code.Common.Cameras
{
    public class CinemachineCameraService : MonoBehaviour, ICameraService
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CinemachineCamera[] _cinemachineCameras;

        private int _activeCameraIndex = 0;
        private int _unActiveCameraIndex = -1;

        public Camera GetCamera() => _camera;

        public void SetActiveCamera(int index)
        {
            for (var i = 0; i < _cinemachineCameras.Length; i++)
                _cinemachineCameras[i].Priority = _unActiveCameraIndex;

            _cinemachineCameras[index].Priority = _activeCameraIndex;   
        }
    }
}