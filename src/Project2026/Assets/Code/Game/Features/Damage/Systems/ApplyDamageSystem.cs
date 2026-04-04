using Entitas;

namespace Code.Game.Features.Damage.Systems
{
    public class ApplyDamageSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        public ApplyDamageSystem(GameContext gameContext)
        {
            _damages = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Damage,
                    GameMatcher.DamageRequest,
                    GameMatcher.TargetId));
        }

        public void Execute()
        {
            foreach (var damage in _damages)
            {
                var target = damage.targetId.Value;
                var damageAmount = damage.damage.Value;

                //if (target.hasHealth)
                //{
                //    target.ReplaceHealth(target.health.Value - damageAmount);
                //}
                //damage.isDestroyed = true;
            }
        }
    }
}