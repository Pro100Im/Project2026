using UnityEngine;

namespace Code.Game.Common.Cameras
{
    public interface ICameraService
    {
        public Camera GetCamera();

        public void SetActiveTownCamera();
        public void SetActiveMainCamera();
    }
}