using Code.Meta.Features.Game;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerMenuSystem : ReactiveSystem<InputEntity>
    {
        private readonly GameScreen _gameScreen;

        public TowerMenuSystem(InputContext inputContext, GameScreen gameScreen)
            : base(inputContext) => _gameScreen = gameScreen;

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(InputMatcher
                .AllOf(
                InputMatcher.Input,
                InputMatcher.TargetId,
                InputMatcher.ScreenPointerInput
                ));

        protected override bool Filter(InputEntity entity) => entity.isInput && entity.hasTargetId && entity.hasScreenPointerInput;

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (var entity in entities)
            {
                
            }
        }
    }
}