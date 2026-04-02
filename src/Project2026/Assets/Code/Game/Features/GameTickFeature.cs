using Code.Game.Features.Animator;
using Code.Game.Features.Attack;
using Code.Game.Features.Input;
using Code.Game.Features.Movement;
using Code.Game.Features.Player;
using Code.Game.Features.Spawn;
using Code.Game.Features.Wave;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;

namespace Code.Game.Features
{
    public class GameTickFeature : Feature
    {
        public GameTickFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<InputFeature>());
            Add(systemFactory.Create<PlayerFeature>());

            Add(systemFactory.Create<WaveFeature>());
            Add(systemFactory.Create<SpawnFeature>());
            Add(systemFactory.Create<CreateViewFeature>());
            Add(systemFactory.Create<AnimatorFeature>());
            Add(systemFactory.Create<MovementFeature>());
            Add(systemFactory.Create<AttackFeature>());
        }
    }
}
