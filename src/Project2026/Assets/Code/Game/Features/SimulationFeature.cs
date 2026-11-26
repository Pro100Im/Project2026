using Code.Game.Common.Time;
using Code.Game.Features.Level;
using Code.Game.Features.Movement;
using Code.Game.Features.Target;
using Code.Infrastructure.Systems;

namespace Code.Game.Features
{
    /// <summary>
    /// Decisions and movement of units run at a fixed step, so behaviour does not depend on the frame rate.
    /// A frame executes zero or more steps; views are interpolated between the last two (see MovementFeature).
    /// Everything else in the game keeps running once per frame.
    /// </summary>
    public class SimulationFeature : Feature
    {
        public const float Step = 1f / 30f;

        private const int MaxStepsPerFrame = 3;

        private readonly ITimeService _timeService;

        private float _accumulator;

        public SimulationFeature(ISystemFactory systemFactory, ITimeService timeService)
        {
            _timeService = timeService;

            Add(systemFactory.Create<LevelFeature>());
            Add(systemFactory.Create<TargetFeature>());
            Add(systemFactory.Create<CrowdFeature>());
        }

        public override void Execute()
        {
            _accumulator += _timeService.DeltaTime;

            var steps = 0;

            while (_accumulator >= Step && steps < MaxStepsPerFrame)
            {
                _timeService.BeginSimulationStep(Step);
                base.Execute();
                _timeService.EndSimulationStep();

                _accumulator -= Step;
                steps++;
            }

            // A frame too long to catch up with: drop the rest instead of spiralling.
            if (_accumulator > Step)
                _accumulator = Step;

            _timeService.SimulationAlpha = _accumulator / Step;
        }
    }
}
