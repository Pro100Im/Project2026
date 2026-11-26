using Code.Game.Common.Time;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    /// <summary>
    /// Tracks where units stand still. The value is smoothed over about a second, so a passing stream leaves no
    /// trace while a fight or a jam that lasts builds up and makes the navigation fields route around it.
    /// </summary>
    public class UpdateCongestionSystem : IExecuteSystem
    {
        private const float SmoothTime = 1f;
        private const float MaxCongestion = 4f;

        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _maps;

        public UpdateCongestionSystem(ITimeService timeService)
        {
            _timeService = timeService;
            _maps = Contexts.sharedInstance.game.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;
            var deltaTime = _timeService.DeltaTime;
            var decay = Mathf.Exp(-deltaTime / SmoothTime);
            var congestion = arena.Congestion;

            for (var cell = 0; cell < arena.CellCount; cell++)
            {
                var value = congestion[cell];

                if (value > 0f)
                    congestion[cell] = value < 0.01f ? 0f : value * decay;
            }

            // One unit standing in a cell settles at a value of about 1.
            var gain = deltaTime / SmoothTime;

            for (var slot = 0; slot < arena.SlotLimit; slot++)
            {
                if (arena.UnitAlive[slot] == 0 || arena.UnitMoving[slot] != 0)
                    continue;

                var position = arena.UnitPos[slot];
                var cell = arena.IndexAt(position.x, position.y);

                if (cell >= 0)
                    congestion[cell] = Mathf.Min(congestion[cell] + gain, MaxCongestion);
            }
        }
    }
}
