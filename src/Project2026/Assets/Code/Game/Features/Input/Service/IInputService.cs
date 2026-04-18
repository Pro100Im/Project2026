using UnityEngine;

namespace Code.Game.Input.Service
{
    public interface IInputService
    {
        public void EnableInput();
        public void DisableInput();

        public bool WasClicked();

        public Vector2 GetPointer();
        public Vector2 GetWorldPointer();
        public Vector2 GetScreenPointer(Vector3 pos);

        public Ray GetRayWorldPointer();
    }
}