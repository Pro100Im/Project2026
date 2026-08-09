using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Target.Systems
{
    public class AssignRallyToCastleSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _castles;
        private readonly IGroup<GameEntity> _idleDefenders;
        private readonly IGroup<GameEntity> _rallyingDefenders;

        private readonly List<GameEntity> _buffer = new(256);

        public AssignRallyToCastleSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _castles = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.PlayerCastle)
                .NoneOf(
                    GameMatcher.Dead));

            _idleDefenders = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Team,
                    GameMatcher.GridMovement,
                    GameMatcher.MovementAvailable)
                .NoneOf(
                    GameMatcher.Dead,
                    GameMatcher.SurroundSlot,
                    GameMatcher.TargetId,
                    GameMatcher.Attacking,
                    GameMatcher.RallyToCastle));

            _rallyingDefenders = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.RallyToCastle));
        }

        public void Execute()
        {
            var castle = _castles.GetSingleEntity();

            if (castle == null)
                return;

            if (castle.isCastleUnderAttack)
            {
                var defenders = _idleDefenders.GetEntities(_buffer);

                for (var i = 0; i < defenders.Count; i++)
                {
                    var defender = defenders[i];

                    if (defender.team.Value != Team.Player)
                        continue;

                    defender.isRallyToCastle = true;
                }
            }
            else
            {
                var rallying = _rallyingDefenders.GetEntities(_buffer);

                for (var i = 0; i < rallying.Count; i++)
                    rallying[i].isRallyToCastle = false;
            }
        }
    }
}
