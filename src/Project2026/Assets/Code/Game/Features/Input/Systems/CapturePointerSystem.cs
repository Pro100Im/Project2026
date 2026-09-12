using Code.Game.Common.UI;
using Code.Game.Input.Service;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Input.Systems
{
    public class CapturePointerSystem : IExecuteSystem
    {
        private readonly IInputService _inputService;
        private readonly UIService _uiService;
        private readonly IGroup<InputEntity> _pointers;

        private readonly List<InputEntity> _pointersBuffer = new(1);

        public CapturePointerSystem(IInputService inputService, UIService uiService)
        {
            _inputService = inputService;
            _uiService = uiService;

            _pointers = Contexts.sharedInstance.input.GetGroup(InputMatcher
                .AllOf(InputMatcher.PointerState)
                .NoneOf(InputMatcher.Destructed));
        }

        public void Execute()
        {
            var pointers = _pointers.GetEntities(_pointersBuffer);
            var screenPointer = _inputService.GetPointer();
            var worldPointer = _inputService.GetWorldPointer();
            var overUI = _uiService.IsPointerOverUI(screenPointer);

            for (var i = 0; i < pointers.Count; i++)
            {
                var pointer = pointers[i];

                pointer.ReplacePointerInput(screenPointer);
                pointer.ReplaceWorldPointerInput(worldPointer);
                pointer.isPointerOverUI = overUI;
            }
        }
    }
}
