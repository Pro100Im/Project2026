using Code.Game.Common.Cameras;
using Settings.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Game.Input.Service
{
    // To do rework
    public class InputService : IInputService
    {
        private readonly ICameraService _cameraService;
        private readonly NewInputSystemApi _newInputSystemApi;

        public InputService(ICameraService cameraService)
        {
            _newInputSystemApi = new NewInputSystemApi();
            _cameraService = cameraService;
        }

        public Vector2 GetPointer() => _newInputSystemApi.Player.Point.ReadValue<Vector2>();

        public Vector2 GetWorldPointer()
        {
            if(Mouse.current == null)
                return Vector2.zero;

            return _cameraService.GetCamera().ScreenToWorldPoint(GetPointer());
        }

        public Ray GetRayWorldPointer()
        {
            if (Mouse.current == null)
                return new Ray();

            return _cameraService.GetCamera().ScreenPointToRay(GetPointer());
        }

        public void EnableInput() => _newInputSystemApi.Player.Enable();
        public void DisableInput() => _newInputSystemApi.Player.Disable();

        public bool HasAxisInput() => GetInputAxis().magnitude > 0;

        public float GetVerticalAxis() => GetInputAxis().y;

        public float GetHorizontalAxis() => GetInputAxis().x;

        private Vector2 GetInputAxis() => _newInputSystemApi.Player.Move.ReadValue<Vector2>();
    }
}