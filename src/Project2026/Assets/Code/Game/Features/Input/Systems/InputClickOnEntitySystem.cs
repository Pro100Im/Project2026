using Code.Game.Common.Entity;
using Code.Game.Input.Service;
using Code.Meta.Features.Game;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Input.Systems
{
    public class InputClickOnEntitySystem : IExecuteSystem
    {
        private readonly IInputService _inputService;
        private readonly GameScreen _gameScreen;

        private readonly IGroup<GameEntity> _entities;
        private readonly IGroup<GameEntity> _gameSessions;

        private readonly List<GameEntity> _entitiesBuffer = new(16);

        public InputClickOnEntitySystem(GameContext gameContext, IInputService inputService, GameScreen gameScreen)
        {
            _inputService = inputService;
            _gameScreen = gameScreen;

            _entities = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Id,
                GameMatcher.TouchZone,
                GameMatcher.Transform));

            _gameSessions = gameContext.GetGroup(GameMatcher.GameSession);
        }

        public void Execute()
        {
            var session = _gameSessions.GetSingleEntity();
            if (session != null && session.isPause)
                return;

            if (_inputService.WasClicked())
            {
                var pointer = _inputService.GetPointer();

                if (!_gameScreen.IsPointerOverUI(pointer))
                {
                    var worldPos = _inputService.GetWorldPointer();
                    var entityClick = CreateInputEntity.Empty();
                    var entities = _entities.GetEntities(_entitiesBuffer);

                    entityClick.isInput = true;

                    for (var i = 0; i < entities.Count; i++)
                    {
                        var entity = entities[i];

                        if (!entity.touchZone.Value.bounds.Contains(worldPos))
                            continue;

                        var screenPoint = _inputService.GetScreenPointer(entity.woldPos.Value);

                        entityClick.AddScreenPointerInput(screenPoint);
                        entityClick.AddTargetId(entity.id.Value);

                        break;
                    }
                }
            }
        }
    }
}