using UnityEngine;

namespace Code.Common.Cameras
{
    public interface ICameraService
    {
        Camera GetCamera();

        void SetActiveCamera(int index);
    }
}