using Entitas;

namespace Code.Game.Features.Enemy.Systems
{
    public class EnemyAnimatorSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _enemies;

        public EnemyAnimatorSystem(GameContext gameContext)
        {
            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Enemy,
                GameMatcher.Animator));
        }

        public void Execute()
        {
            foreach (var enemy in _enemies)
            {
                
            }
        }
    }
}
