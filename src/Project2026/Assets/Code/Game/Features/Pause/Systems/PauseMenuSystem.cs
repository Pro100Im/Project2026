using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Pause.Systems
{
    public class PauseMenuSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<MetaEntity> _pauseMenu;

        public PauseMenuSystem(GameContext gameContext, MetaContext metaContext) : base(gameContext)
        {
            _pauseMenu = metaContext.GetGroup(MetaMatcher.PauseMenu);
        }

        protected override void Execute(List<GameEntity> entities)
        {
            var pauseMenuEntity = _pauseMenu.GetSingleEntity();

            if (pauseMenuEntity == null)
                return;

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.isPause)
                    pauseMenuEntity.pauseMenu.Value.OpenMenu();
                else
                    pauseMenuEntity.pauseMenu.Value.CloseMenu();
            }
        }

        protected override bool Filter(GameEntity entity) => entity.isGameSession;

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) => context.CreateCollector(GameMatcher.Pause.AddedOrRemoved());
    }
}