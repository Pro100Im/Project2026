using Code.Common.Time;
using Entitas;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackProcessSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _attackers;

        public AttackProcessSystem(GameContext gameContext, ITimeService timeService)
        {
            _timeService = timeService;

            //_attackers = gameContext.GetGroup(GameMatcher
            //    .AllOf(
            //    GameMatcher.TargetId,
            //    GameMatcher.Attacking));
        }

        public void Execute()
        {

        }
    }
}