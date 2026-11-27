using Code.Game.Features.Level;
using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    /// <summary>
    /// Runs every rendered frame: places unit views between the previous and the current simulation position,
    /// so movement is smooth at any frame rate although the simulation ticks at a fixed step.
    /// </summary>
    public class InterpolateUnitViewSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _units;

        private readonly List<GameEntity> _buffer = new(LevelBuffers.DefaultCapacity);

        public InterpolateUnitViewSystem(ITimeService timeService)
        {
            var gameContext = Contexts.sharedInstance.game;

            _timeService = timeService;
            _maps = gameContext.GetGroup(GameMatcher.Arena);

            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SimSlot,
                    GameMatcher.Transform,
                    GameMatcher.WoldPos));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;
            var alpha = Mathf.Clamp01(_timeService.SimulationAlpha);
            var units = _units.GetEntities(LevelBuffers.Fit(_buffer, arena));

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var slot = unit.simSlot.Value;

                if (arena.UnitAlive[slot] == 0)
                    continue;

                var position = Vector2.LerpUnclamped(arena.UnitPrevPos[slot], arena.UnitPos[slot], alpha)
                    + arena.UnitViewOffset[slot];

                unit.transform.Value.position = new Vector3(position.x, position.y, unit.woldPos.Value.z);
            }
        }
    }
}
