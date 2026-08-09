using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Debaffs.Systems
{
    public class FreezeSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _frozenUnits;

        private readonly List<GameEntity> _frozenUnitsBuffer = new(128);

        public FreezeSystem()
        {
            _frozenUnits = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Unit,
                    GameMatcher.FreezeDebuff)
                .NoneOf(GameMatcher.Dead));
        }

        public void Execute()
        {
            var units = _frozenUnits.GetEntities(_frozenUnitsBuffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];

                if (unit.isMoving)
                    unit.isMoving = false;

                if (unit.hasVelocity)
                    unit.RemoveVelocity();
            }
        }
    }
}
