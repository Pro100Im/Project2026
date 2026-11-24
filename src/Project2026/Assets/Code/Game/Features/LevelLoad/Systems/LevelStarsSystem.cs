using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.LevelLoad.Systems
{
    /// <summary>
    /// On victory rates the run by the castle health left, using the active level thresholds.
    /// </summary>
    public class LevelStarsSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<GameEntity> _castles;
        private readonly List<GameEntity> _castleBuffer = new(2);

        public LevelStarsSystem() : base(Contexts.sharedInstance.game)
        {
            _castles = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.PlayerCastle,
                    GameMatcher.CurrentHealth,
                    GameMatcher.MaxHealth));
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) =>
            context.CreateCollector(GameMatcher.Victory.Added());

        protected override bool Filter(GameEntity entity) =>
            entity.isGameSession && entity.isVictory && entity.hasActiveLevel && !entity.hasLevelStars;

        protected override void Execute(List<GameEntity> entities)
        {
            var ratio = CastleHealthRatio();

            for (var i = 0; i < entities.Count; i++)
            {
                var session = entities[i];
                session.AddLevelStars(session.activeLevel.Value.ResolveStars(ratio));
            }
        }

        private float CastleHealthRatio()
        {
            var castles = _castles.GetEntities(_castleBuffer);
            var current = 0;
            var max = 0;

            for (var i = 0; i < castles.Count; i++)
            {
                current += castles[i].currentHealth.Value;
                max += castles[i].maxHealth.Value;
            }

            return max > 0 ? (float)current / max : 1f;
        }
    }
}
