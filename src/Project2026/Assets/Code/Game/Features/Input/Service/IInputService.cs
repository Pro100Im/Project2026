using UnityEngine;

namespace Code.Game.Input.Service
{
    public interface IInputService
    {
        void EnableInput();
        void DisableInput();

        float GetVerticalAxis();
        float GetHorizontalAxis();

        bool HasAxisInput();

        Vector2 GetPointer();
        Vector2 GetWorldPointer();

        Ray GetRayWorldPointer();
    }
}