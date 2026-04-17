using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Game.Input.Service
{
    public interface IInputService
    {
        public void SubscribeOnClick(Action<InputAction.CallbackContext> clickAction);

        public void UnSubscribeOnClick(Action<InputAction.CallbackContext> clickAction);

        public void EnableInput();
        public void DisableInput();

        public Vector2 GetPointer();
        public Vector2 GetWorldPointer();
        public Vector2 GetScreenPointer(Vector3 pos);

        public Ray GetRayWorldPointer();
    }
}