using Code.Game.Common.Entity;
using Code.Game.Input.Service;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Input.Systems
{
    public class CaptureClickSystem : IExecuteSystem
    {
        private readonly IInputService _inputService;
        private readonly IGroup<InputEntity> _pointers;
        private readonly IGroup<GameEntity> _gameSessions;

        private readonly List<InputEntity> _pointersBuffer = new(1);

        public CaptureClickSystem(IInputService inputService)
        {
            _inputService = inputService;

            _pointers = Contexts.sharedInstance.input.GetGroup(InputMatcher
                .AllOf(
                    InputMatcher.PointerState,
                    InputMatcher.PointerInput,
                    InputMatcher.WorldPointerInput)
                .NoneOf(InputMatcher.Destructed));

            _gameSessions = Contexts.sharedInstance.game.GetGroup(GameMatcher.GameSession);
        }

        public void Execute()
        {
            var session = _gameSessions.GetSingleEntity();
            if (session != null && session.isPause)
                return;

            var primaryClicked = _inputService.WasClicked();
            var cancelClicked = _inputService.WasCancelClicked();

            if (!primaryClicked && !cancelClicked)
                return;

            var pointers = _pointers.GetEntities(_pointersBuffer);
            if (pointers.Count == 0)
                return;

            var pointer = pointers[0];
            var click = CreateInputEntity.Empty();

            click.isInput = true;
            click.isClickInput = true;
            click.AddPointerInput(pointer.pointerInput.Value);
            click.AddWorldPointerInput(pointer.worldPointerInput.Value);
            click.isPointerOverUI = pointer.isPointerOverUI;

            if (primaryClicked)
                click.isPrimaryClick = true;

            if (cancelClicked)
                click.isCancelClick = true;
        }
    }
}
