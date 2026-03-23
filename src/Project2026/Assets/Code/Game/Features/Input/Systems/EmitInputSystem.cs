using Code.Game.Input.Service;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Input.Systems
{
    public class EmitInputSystem : IExecuteSystem
    {
        private readonly IInputService _inputService;
        private readonly IGroup<InputEntity> _inputs;

        public EmitInputSystem(InputContext input, IInputService inputService)
        {
            _inputService = inputService;
            _inputs = input.GetGroup(InputMatcher.Input);
        }

        public void Execute()
        {
            foreach (InputEntity input in _inputs)
            {
                if (!input.isLocalPlayer)
                    continue;

                if (_inputService.HasAxisInput())
                    input.ReplaceAxisInput(new Vector2(_inputService.GetHorizontalAxis(), _inputService.GetVerticalAxis()));
                else
                    input.ReplaceAxisInput(Vector2.zero);

                input.ReplacePointerInput(_inputService.GetWorldPointer());
                input.ReplacePointerRay(_inputService.GetRayWorldPointer());
            }
        }
    }
}