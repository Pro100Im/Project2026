using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.GameSession.Systems
{
    public class GameSessionEndMenuSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<MetaEntity> _endGameMenu;

        public GameSessionEndMenuSystem() : base(Contexts.sharedInstance.game)
        {
            _endGameMenu = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.EndGameMenu);
        }

        protected override void Execute(List<GameEntity> entities)
        {
            var endGameMenuEntity = _endGameMenu.GetSingleEntity();

            if (endGameMenuEntity == null)
                return;

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.isForcedPause)
                    endGameMenuEntity.endGameMenu.Value.OpenMenu();
                else
                    endGameMenuEntity.endGameMenu.Value.CloseMenu();
            }
        }

        protected override bool Filter(GameEntity entity) => entity.isGameSession;

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) => context.CreateCollector(GameMatcher.ForcedPause.AddedOrRemoved());
    }
}
