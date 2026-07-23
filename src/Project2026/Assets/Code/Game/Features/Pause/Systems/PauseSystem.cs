using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Pause.Systems
{
    public class PauseSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<GameEntity> _game;
        private readonly ITimeService _timeService;

        public PauseSystem(InputContext inputContext, GameContext gameContext, ITimeService timeService) : base(inputContext)
        {
            _game = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GameSession));

            _timeService = timeService;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            var game = _game.GetSingleEntity();

            if (game == null)
                return;

            if (game.isPause)
            {
                game.isPause = false;

                _timeService.StartTime();

            }
            else
            {
                game.isPause = true;

                _timeService.StopTime();
            }

            foreach (var entity in entities)
                entity.isPauseRequested = false;
        }

        protected override bool Filter(InputEntity entity) => entity.isPauseRequested;

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
           context.CreateCollector(InputMatcher.PauseRequested.Added());
    }
}