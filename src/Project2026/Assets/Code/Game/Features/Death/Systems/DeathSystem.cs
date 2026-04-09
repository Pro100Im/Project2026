using Entitas;

namespace Code.Game.Features.Death.Systems
{
    public class DeathSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;

        public DeathSystem(GameContext gameContext)
        {
            _entities = gameContext.GetGroup(GameMatcher.CurrentHealth);
        }

        public void Execute()
        {
            foreach (var entity in _entities)
            {
                if(!entity.isDead && entity.currentHealth.Value <= 0)
                {
                    entity.isDead = true;
                }
            }
        }
    }
}