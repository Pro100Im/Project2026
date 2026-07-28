using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Ability.Systems
{
    public class AbilitySelectSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _requests;
        private readonly IGroup<GameEntity> _targeting;

        private readonly List<GameEntity> _requestsBuffer = new(8);
        private readonly List<GameEntity> _targetingBuffer = new(2);

        public AbilitySelectSystem(GameContext gameContext)
        {
            _requests = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.AbilitySelectRequest,
                    GameMatcher.EntityConfig)
                .NoneOf(GameMatcher.Destructed));

            _targeting = gameContext.GetGroup(GameMatcher
                .AllOf(GameMatcher.AbilityTargeting)
                .NoneOf(GameMatcher.Destructed));
        }

        public void Execute()
        {
            var requests = _requests.GetEntities(_requestsBuffer);

            for (var i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                var targeting = _targeting.GetEntities(_targetingBuffer);

                if (targeting.Count > 0)
                {
                    for (var j = 0; j < targeting.Count; j++)
                    {
                        var entity = targeting[j];

                        entity.isAbilityTargeting = false;
                        entity.isAbilityRangeShowed = false;
                        entity.isDestructed = true;
                    }
                }
                else
                {
                    CreateTargetingEntity(request);
                }

                request.isDestructed = true;
            }
        }

        private static void CreateTargetingEntity(GameEntity request)
        {
            var entity = CreateGameEntity.Empty();

            foreach (var property in request.entityConfig.Value.Properties)
                property.Apply(entity);

            entity.isAbilityTargeting = true;
        }
    }
}
