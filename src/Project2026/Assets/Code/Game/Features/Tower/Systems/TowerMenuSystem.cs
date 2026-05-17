using Code.Game.Common.Entity;
using Code.Meta.Features.Game;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerMenuSystem : ReactiveSystem<InputEntity>
    {
        private readonly GameScreen _gameScreen;

        private readonly IGroup<GameEntity> _towers;

        private readonly List<GameEntity> _towersBuffer = new(16);

        public TowerMenuSystem(InputContext inputContext, GameContext gameContext, GameScreen gameScreen)
            : base(inputContext)
        {
            _gameScreen = gameScreen;

            _towers = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Player,
                GameMatcher.Tower,
                GameMatcher.LineRenderer,
                GameMatcher.EntityConfig));
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(InputMatcher
                .AllOf(
                InputMatcher.Input
                ));

        protected override bool Filter(InputEntity entity) => entity.isInput;

        protected override void Execute(List<InputEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.hasTargetId)
                {
                    var targetEntity = GetGameEntityById.Get(entity.targetId.Value);

                    if (targetEntity.isTowerPlace)
                    {
                        _gameScreen.OpenTowerBuildMenu(entity.screenPointerInput.Value, targetEntity);


                    }

                    //var towers = _towers.GetEntities(_towersBuffer);

                    //for (var j = 0; j < towers.Count; j++)
                    //{
                    //    var tower = towers[j];

                    //    if (tower.id.Value != entity.targetId.Value)
                    //    {

                    //    }

                    //    if (targetEntity.isTower && targetEntity.hasRange)
                    //    {



                    //    }
                    //}

                    entity.isDestructed = true;
                }
                else
                {
                    Debug.Log("Close menu");
                    _gameScreen.CloseTowerBuildMenu();
                }

                entity.isDestructed = true;
            }
        }
    }
}