using Code.Game.Common.Entity;
using Code.Game.Input.Service;
using Code.Meta.Features.Game;
using Entitas;

namespace Code.Game.Features.Input.Systems
{
    public class InputClickOnEntitySystem : IExecuteSystem
    {
        private readonly IInputService _inputService;
        private readonly GameScreen _gameScreen;

        private readonly IGroup<GameEntity> _entities;

        public InputClickOnEntitySystem(GameContext gameContext, IInputService inputService, GameScreen gameScreen)
        {
            _inputService = inputService;
            _gameScreen = gameScreen;

            _entities = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Id,
                GameMatcher.TouchZone,
                GameMatcher.Transform));
        }

        public void Execute()
        {
            if (_inputService.WasClicked())
            {
                var pointer = _inputService.GetPointer();

                if (!_gameScreen.IsPointerOverUI(pointer))
                {
                    var worldPos = _inputService.GetWorldPointer();

                    foreach (var entity in _entities)
                    {
                        if (!entity.touchZone.Value.bounds.Contains(worldPos))
                            continue;

                        var screenPoint = _inputService.GetScreenPointer(entity.transform.Value.position);
                        var entityClick = CreateInputEntity.Empty();

                        entityClick.isInput = true;
                        entityClick.AddTargetId(entity.id.Value);
                        entityClick.AddScreenPointerInput(screenPoint);

                        break;
                    }
                }
            }
        }
    }
}