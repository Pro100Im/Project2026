using Code.Game.Common.Cameras;
using Settings.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Game.Input.Service
{
    public class InputService : IInputService
    {
        private readonly ICameraService _cameraService;
        private readonly NewInputSystemApi _newInputSystemApi;

        public InputService(ICameraService cameraService)
        {
            _newInputSystemApi = new NewInputSystemApi();
            _cameraService = cameraService;
        }

        public bool WasClicked() => _newInputSystemApi.Player.PointClick.WasPerformedThisFrame();
        public bool WasCancelClicked() => _newInputSystemApi.Player.Cancel.WasPerformedThisFrame();
        public bool WasPauseClicked() => _newInputSystemApi.Player.Pause.WasPerformedThisFrame();

        public Vector2 GetPointer() => _newInputSystemApi.Player.Point.ReadValue<Vector2>();

        public Vector2 GetWorldPointer()
        {
            if(Mouse.current == null || _cameraService.GetCamera() == null)
                return Vector2.zero;

            return _cameraService.GetCamera().ScreenToWorldPoint(GetPointer());
        }

        public Vector2 GetScreenPointer(Vector3 pos)
        {
            if (Mouse.current == null || _cameraService.GetCamera() == null)
                return Vector2.zero;

            return _cameraService.GetCamera().WorldToScreenPoint(pos);
        }

        public void EnableInput() => _newInputSystemApi.Player.Enable();
        public void DisableInput() => _newInputSystemApi.Player.Disable();
    }
}
