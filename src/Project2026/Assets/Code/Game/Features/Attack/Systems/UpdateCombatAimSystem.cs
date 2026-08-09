using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class UpdateCombatAimSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _aimers;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);

        public UpdateCombatAimSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _aimers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.TargetId,
                    GameMatcher.Team,
                    GameMatcher.Range,
                    GameMatcher.DetectionRange,
                    GameMatcher.CurrentCell,
                    GameMatcher.Transform)
                .NoneOf(
                    GameMatcher.TrajectoryMovement,
                    GameMatcher.Dead));

            _maps = gameContext.GetGroup(GameMatcher.AllOf(GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var tilemap = mapEntity.tilemapMovement.Value;
            var aimers = _aimers.GetEntities(_buffer);

            for (var i = 0; i < aimers.Count; i++)
            {
                var aimer = aimers[i];
                var target = GetGameEntityById.Get(aimer.targetId.Value);

                if (target == null || target.isDead || !target.hasCurrentCell)
                    continue;

                var attackOriginPos = aimer.woldPos.Value;

                if (aimer.hasUnitAnchorPoint)
                    attackOriginPos += aimer.unitAnchorPoint.Value;

                var targetPoint = TargetService.GetClosestPoint(target, tilemap, attackOriginPos);

                aimer.ReplaceAttackerPoint(attackOriginPos);
                aimer.ReplaceTargetPoint(targetPoint);
            }
        }
    }
}
