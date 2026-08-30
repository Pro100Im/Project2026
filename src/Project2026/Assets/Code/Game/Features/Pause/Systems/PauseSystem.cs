using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Pause.Systems
{
    public class PauseSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<GameEntity> _game;
        private readonly IGroup<GameEntity> _animators;
        private readonly ITimeService _timeService;
        private readonly List<GameEntity> _animatorsBuffer = new(512);

        public PauseSystem(ITimeService timeService) : base(Contexts.sharedInstance.input)
        {
            var gameContext = Contexts.sharedInstance.game;

            _game = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GameSession));

            _animators = gameContext.GetGroup(GameMatcher.Animator);
            _timeService = timeService;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            var game = _game.GetSingleEntity();

            if (game == null)
                return;

            var forcedPauseRequested = false;
            var pauseRequested = false;

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.isForcedPauseRequested)
                    forcedPauseRequested = true;

                if (entity.isPauseRequested)
                    pauseRequested = true;

                entity.isForcedPauseRequested = false;
                entity.isPauseRequested = false;
                entity.isDestructed = true;
            }

            if (forcedPauseRequested)
            {
                if (!game.isForcedPause)
                {
                    game.isForcedPause = true;
                    game.isPause = false;
                    PauseGame();
                }

                return;
            }

            if (!pauseRequested || game.isForcedPause)
                return;

            if (game.isPause)
            {
                game.isPause = false;
                ResumeGame();
            }
            else
            {
                game.isPause = true;
                PauseGame();
            }
        }

        protected override bool Filter(InputEntity entity) => entity.isPauseRequested || entity.isForcedPauseRequested;

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
           context.CreateCollector(InputMatcher.PauseRequested.Added(), InputMatcher.ForcedPauseRequested.Added());

        private void PauseGame()
        {
            _timeService.StopTime();
            SetAnimatorsSpeed(0f);
        }

        private void ResumeGame()
        {
            _timeService.StartTime();
            SetAnimatorsSpeed(1f);
        }

        private void SetAnimatorsSpeed(float speed)
        {
            var animators = _animators.GetEntities(_animatorsBuffer);

            for (var i = 0; i < animators.Count; i++)
            {
                var animator = animators[i].animator.Value;
                if (animator != null)
                    animator.speed = speed;
            }
        }
    }
}
