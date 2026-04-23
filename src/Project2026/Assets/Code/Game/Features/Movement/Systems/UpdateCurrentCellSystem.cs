using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class UpdateCurrentCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;

        public UpdateCurrentCellSystem(GameContext gameContext)
        {
            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.CurrentCell));
        }

        public void Execute()
        {
            foreach (var unit in _units)
            {
                var pos = unit.transform.Value.position;
                var cell = new Vector3Int(Mathf.FloorToInt(pos.x),Mathf.FloorToInt(pos.y),0);

                unit.ReplaceCurrentCell(cell);
            }
        }
    }
}