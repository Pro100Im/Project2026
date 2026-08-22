using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.GameSession.Systems
{
    public class GameSessionSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<GameEntity> _tower;
        private readonly IGroup<GameEntity> _gameSession;
        //private readonly IGroup<MetaEntity> _pauseMenu;

        //private readonly List<GameEntity> _hpBarsBuffer = new(512);

        public GameSessionSystem() : base(Contexts.sharedInstance.game)
        {
            //_pauseMenu = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.PauseMenu);
            _gameSession = Contexts.sharedInstance.game.GetGroup(GameMatcher.GameSession);

            _tower = Contexts.sharedInstance.game.GetGroup(GameMatcher
               .AllOf(
                   GameMatcher.CurrentHealth,
                   GameMatcher.MaxHealth,
                   GameMatcher.PlayerCastle));
        }

        protected override void Execute(List<GameEntity> entities)
        {
            var entity = CreateInputEntity.Empty();

            entity.isInput = true;
            entity.isForcedPauseRequested = true;
        }

        protected override bool Filter(GameEntity entity) => entity.hasCurrentHealth && entity.hasMaxHealth && entity.isPlayerCastle;

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) => context.CreateCollector(GameMatcher.Dead.Added());
    }
}