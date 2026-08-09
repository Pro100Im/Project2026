using Code.Game.Input.Service;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Input.Systems
{
    public class ResolveClickTargetSystem : IExecuteSystem
    {
        private readonly IInputService _inputService;
        private readonly IGroup<InputEntity> _clicks;
        private readonly IGroup<GameEntity> _entities;

        private readonly List<InputEntity> _clicksBuffer = new(4);
        private readonly List<GameEntity> _entitiesBuffer = new(16);

        public ResolveClickTargetSystem(IInputService inputService)
        {
            _inputService = inputService;

            _clicks = Contexts.sharedInstance.input.GetGroup(InputMatcher
                .AllOf(
                    InputMatcher.ClickInput,
                    InputMatcher.PrimaryClick,
                    InputMatcher.WorldPointerInput)
                .NoneOf(
                    InputMatcher.PointerOverUI,
                    InputMatcher.Destructed));

            _entities = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.TouchZone,
                    GameMatcher.Transform));
        }

        public void Execute()
        {
            var clicks = _clicks.GetEntities(_clicksBuffer);

            for (var i = 0; i < clicks.Count; i++)
            {
                var click = clicks[i];
                var worldPos = click.worldPointerInput.Value;
                var entities = _entities.GetEntities(_entitiesBuffer);

                for (var j = 0; j < entities.Count; j++)
                {
                    var entity = entities[j];

                    if (!entity.touchZone.Value.bounds.Contains(worldPos))
                        continue;

                    var screenPoint = _inputService.GetScreenPointer(entity.woldPos.Value);

                    click.AddScreenPointerInput(screenPoint);
                    click.AddTargetId(entity.id.Value);

                    if (entity.isInteractable)
                        click.isInteractTarget = true;

                    break;
                }
            }
        }
    }
}
