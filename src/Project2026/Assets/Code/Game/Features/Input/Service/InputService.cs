using Code.Game.Common.Cameras;
using Settings.Input;
using System;
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

        public void SubscribeOnClick(Action<InputAction.CallbackContext> clickAction)
        {
            _newInputSystemApi.Player.PointClick.performed += clickAction;
        }

        public bool WasClicked() => _newInputSystemApi.Player.PointClick.WasPerformedThisFrame();

        public Vector2 GetPointer() => _newInputSystemApi.Player.Point.ReadValue<Vector2>();

        public Vector2 GetWorldPointer()
        {
            if(Mouse.current == null)
                return Vector2.zero;

            return _cameraService.GetCamera().ScreenToWorldPoint(GetPointer());
        }

        public Vector2 GetScreenPointer(Vector3 pos)
        {
            if (Mouse.current == null)
                return Vector2.zero;

            return _cameraService.GetCamera().WorldToScreenPoint(pos);
        }

        public Ray GetRayWorldPointer()
        {
            if (Mouse.current == null)
                return new Ray();

            return _cameraService.GetCamera().ScreenPointToRay(GetPointer());
        }

        public void EnableInput() => _newInputSystemApi.Player.Enable();
        public void DisableInput() => _newInputSystemApi.Player.Disable();
    }
}