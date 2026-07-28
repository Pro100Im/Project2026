using Code.Game.Input.Service;
using Code.Meta.Features.Game;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Input.Systems
{
    public class CapturePointerSystem : IExecuteSystem
    {
        private readonly IInputService _inputService;
        private readonly GameScreen _gameScreen;
        private readonly IGroup<InputEntity> _pointers;

        private readonly List<InputEntity> _pointersBuffer = new(1);

        public CapturePointerSystem(
            InputContext inputContext,
            IInputService inputService,
            GameScreen gameScreen)
        {
            _inputService = inputService;
            _gameScreen = gameScreen;

            _pointers = inputContext.GetGroup(InputMatcher
                .AllOf(InputMatcher.PointerState)
                .NoneOf(InputMatcher.Destructed));
        }

        public void Execute()
        {
            var pointers = _pointers.GetEntities(_pointersBuffer);
            var screenPointer = _inputService.GetPointer();
            var worldPointer = _inputService.GetWorldPointer();
            var overUI = _gameScreen.IsPointerOverUI(screenPointer);

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
