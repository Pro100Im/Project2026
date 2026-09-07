using Code.Game.Features.Tower;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Attack.Systems
{
    public class UpdateCannonAimSystem : IExecuteSystem
    {
        private const float RightAngleThreshold = 22.5f;
        private const float DiagonalAngleThreshold = 67.5f;

        private readonly IGroup<GameEntity> _cannons;
        private readonly List<GameEntity> _buffer = new(64);

        public UpdateCannonAimSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _cannons = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Tower,
                    GameMatcher.CannonAnimator,
                    GameMatcher.TargetId,
                    GameMatcher.AttackerPoint,
                    GameMatcher.TargetPoint));
        }

        public void Execute()
        {
            var cannons = _cannons.GetEntities(_buffer);

            for (var i = 0; i < cannons.Count; i++)
            {
                var cannon = cannons[i];
                var dx = cannon.targetPoint.Value.x - cannon.attackerPoint.Value.x;
                var dy = cannon.targetPoint.Value.y - cannon.attackerPoint.Value.y;

                if ((dx * dx) + (dy * dy) < 1e-6f)
                    continue;

                var shouldFlipX = dx < 0f;
                var direction = ResolveDirection(Mathf.Atan2(dy, Mathf.Abs(dx)) * Mathf.Rad2Deg);

                if (cannon.hasCannonAimDirection)
                    cannon.ReplaceCannonAimDirection(direction);
                else
                    cannon.AddCannonAimDirection(direction);

                cannon.isCannonFlipX = shouldFlipX;
            }
        }

        private static CannonAimDirection ResolveDirection(float angleDeg)
        {
            if (angleDeg >= DiagonalAngleThreshold)
                return CannonAimDirection.Up;

            if (angleDeg >= RightAngleThreshold)
                return CannonAimDirection.UpRight;

            if (angleDeg > -RightAngleThreshold)
                return CannonAimDirection.Right;

            if (angleDeg > -DiagonalAngleThreshold)
                return CannonAimDirection.DownRight;

            return CannonAimDirection.Down;
        }
    }
}
