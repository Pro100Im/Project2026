using Code.Game.Common.Entity;
using Code.Game.Common.Entity.EntityBuilders;
using Code.Game.Features.Effect.Factory;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.Identifiers;
using UnityEngine;

namespace Code.Game.Features.Enemy.Factory
{
    public class EnemyFactory
    {
        private readonly IIdentifierService _identifiers;
        private readonly EffectFactory _effectFactory;

        public EnemyFactory(IIdentifierService identifiers, EffectFactory effectFactory)
        {
            _identifiers = identifiers;
            _effectFactory = effectFactory;
        }

        public GameEntity Create(EntityConfig entityConfig, Vector3 spawnPosition)
        {
            var entity = new GameEntityBuilder(CreateGameEntity.Empty(), entityConfig)
                .WithId(_identifiers)
                .WithSpawnPosition(spawnPosition)
                .WithView()
                .WithMovement()
                .WithAttack()
                .WithHealth(spawnPosition)
                .WithSpawnEffect(_effectFactory, spawnPosition)
                .WithEnemyTag()
                .WithTargetableTag()
                .Build();

            return entity;
        }
    }
}