using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Target.Systems
{
    public class UpdateCastleThreatSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _castles;
        private readonly IGroup<GameEntity> _enemies;

        private readonly List<GameEntity> _buffer = new(64);

        public UpdateCastleThreatSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _castles = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.PlayerCastle,
                    GameMatcher.Id)
                .NoneOf(
                    GameMatcher.Dead));

            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Team,
                    GameMatcher.Id)
                .NoneOf(
                    GameMatcher.Dead));
        }

        public void Execute()
        {
            var castle = _castles.GetSingleEntity();

            if (castle == null)
                return;

            var castleId = castle.id.Value;
            var threatened = false;
            var enemies = _enemies.GetEntities(_buffer);

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];

                if (enemy.team.Value != Team.Enemy)
                    continue;

                if (enemy.hasSurroundTargetId && enemy.surroundTargetId.Value == castleId)
                {
                    threatened = true;
                    break;
                }

                if (enemy.hasTargetId && enemy.targetId.Value == castleId)
                {
                    threatened = true;
                    break;
                }
            }

            castle.isCastleUnderAttack = threatened;
        }
    }
}
