using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.LevelLoad.Systems
{
    /// <summary>
    /// Saves the rated result of the active level node (best stars are kept by the progress service).
    /// </summary>
    public class LevelCompletionSystem : ReactiveSystem<GameEntity>
    {
        private readonly IUserProgressService _progress;

        public LevelCompletionSystem(IUserProgressService progress) : base(Contexts.sharedInstance.game)
        {
            _progress = progress;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) =>
            context.CreateCollector(GameMatcher.LevelStars.Added());

        protected override bool Filter(GameEntity entity) =>
            entity.isGameSession && entity.hasLevelStars && entity.hasActiveLevelNode;

        protected override void Execute(List<GameEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var session = entities[i];
                _progress.CompleteLevel(session.activeLevelNode.Value, session.levelStars.Value);
            }

            _progress.Flush();
        }
    }
}
