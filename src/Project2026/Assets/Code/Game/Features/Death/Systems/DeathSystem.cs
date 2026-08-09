using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Death.Systems
{
    public class DeathSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _deaths;

        private readonly List<GameEntity> _deathsBuffer = new(86);

        public DeathSystem()
        {
            _deaths = Contexts.sharedInstance.game.GetGroup(GameMatcher.CurrentHealth);
        }

        public void Execute()
        {
            var deaths = _deaths.GetEntities(_deathsBuffer);

            for (var i = 0; i < deaths.Count; i++)
            {
                var entity = deaths[i];

                if (!entity.isDead && entity.currentHealth.Value <= 0)
                {
                    entity.isDead = true;

                    if (entity.hasDeathDuration && entity.hasId)
                    {
                        var destructTimer = CreateGameEntity.Empty();
                        destructTimer.AddTargetId(entity.id.Value);
                        destructTimer.AddDuration(entity.deathDuration.Value);
                        destructTimer.isDelayDestruct = true;
                    }
                }
            }
        }
    }
}
