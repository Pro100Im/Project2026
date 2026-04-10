using Code.Common.Entity;
using Code.Infrastructure.Identifiers;
using Code.Infrastructure.View;
using UnityEngine;

namespace Code.Game.Features.Effect.Factory
{
    public class EffectFactory
    {
        private readonly IIdentifierService _identifierService;

        public EffectFactory(IIdentifierService identifierService)
        {
            _identifierService = identifierService;
        }

        public GameEntity Create(EntityBehaviour prefab, Vector2 spawnPosition)
        {
            var effect = CreateGameEntity.Empty();

            effect.AddId(_identifierService.Next());
            effect.AddViewPrefab(prefab);
            effect.AddSpawnPosition(spawnPosition);

            return effect;
        }
    }
}