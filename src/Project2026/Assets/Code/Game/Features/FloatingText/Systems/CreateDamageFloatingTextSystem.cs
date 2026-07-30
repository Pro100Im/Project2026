using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.FloatingText.Systems
{
    public class CreateDamageFloatingTextSystem : IExecuteSystem
    {
        private readonly FloatingTextConfig _config;
        private readonly IGroup<GameEntity> _damages;
        private readonly List<GameEntity> _damagesBuffer = new(86);

        public CreateDamageFloatingTextSystem(GameContext gameContext, FloatingTextConfig config)
        {
            _config = config;
            _damages = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.DamageRequest,
                    GameMatcher.TotalDamage,
                    GameMatcher.TargetId));
        }

        public void Execute()
        {
            if (_config == null || _config.Text == null || _config.Text.Properties == null)
                return;

            var damages = _damages.GetEntities(_damagesBuffer);

            for (var i = 0; i < damages.Count; i++)
            {
                var damage = damages[i];
                var damageAmount = damage.totalDamage.Value;

                if (damageAmount <= 0f)
                    continue;

                var spawnPos = ResolveSpawnPosition(damage);
                var text = CreateGameEntity.Empty();

                text.AddFloatingText(damageAmount);
                text.isDamageFloatingText = true;
                text.AddSpawnPosition(spawnPos);

                for (var j = 0; j < _config.Text.Properties.Length; j++)
                    _config.Text.Properties[j].Apply(text);
            }
        }

        private Vector3 ResolveSpawnPosition(GameEntity damage)
        {
            if (damage.hasTargetPoint)
                return (Vector3)damage.targetPoint.Value + _config.SpawnOffset;

            var target = GetGameEntityById.Get(damage.targetId.Value);

            if (target != null && target.hasWoldPos)
                return target.woldPos.Value + _config.SpawnOffset;

            if (target != null && target.hasSpawnPosition)
                return target.spawnPosition.Value + _config.SpawnOffset;

            return _config.SpawnOffset;
        }
    }
}
