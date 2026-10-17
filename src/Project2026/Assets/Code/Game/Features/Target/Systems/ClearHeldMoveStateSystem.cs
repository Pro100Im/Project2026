using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Target.Systems
{
    public class ClearHeldMoveStateSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;

        private readonly List<GameEntity> _buffer = new(64);

        public ClearHeldMoveStateSystem()
        {
            _units = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.CurrentCell,
                    GameMatcher.SurroundSlot)
                .AnyOf(
                    GameMatcher.MoveWait,
                    GameMatcher.MoveCommit));
        }

        public void Execute()
        {
            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];

                if (unit.currentCell.Value != unit.surroundSlot.Value)
                    continue;

                if (unit.hasMoveWait)
                    unit.RemoveMoveWait();

                if (unit.hasMoveCommit)
                    unit.RemoveMoveCommit();
            }
        }
    }
}
