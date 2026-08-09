using Code.Game.Common.Destruct;
using Code.Game.Features.Ability;
using Code.Game.Features.Animator;
using Code.Game.Features.Attack;
using Code.Game.Features.Cooldown;
using Code.Game.Features.Damage;
using Code.Game.Features.Death;
using Code.Game.Features.Debaffs;
using Code.Game.Features.Duration;
using Code.Game.Features.Effect;
using Code.Game.Features.Exchequer;
using Code.Game.Features.Health;
using Code.Game.Features.Level;
using Code.Game.Features.Movement;
using Code.Game.Features.Rewards;
using Code.Game.Features.Spawn;
using Code.Game.Features.Target;
using Code.Game.Features.Tower;
using Code.Game.Features.Unit;
using Code.Game.Features.Wave;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;

namespace Code.Game.Features
{
    public class GameplayFeature : Feature
    {
        private readonly IGroup<GameEntity> _gameSessions;

        public GameplayFeature(ISystemFactory systemFactory)
        {
            _gameSessions = Contexts.sharedInstance.game.GetGroup(GameMatcher.GameSession);

            Add(systemFactory.Create<WaveFeature>());
            Add(systemFactory.Create<SpawnFeature>());
            Add(systemFactory.Create<CreateViewFeature>());
            Add(systemFactory.Create<AbilityFeature>());

            Add(systemFactory.Create<DebuffFeature>());

            Add(systemFactory.Create<UnitFeature>());

            Add(systemFactory.Create<TowerFeature>());

            Add(systemFactory.Create<LevelFeature>());

            Add(systemFactory.Create<TargetFeature>());
            Add(systemFactory.Create<MovementFeature>());
            Add(systemFactory.Create<AttackFeature>());
            Add(systemFactory.Create<DamageFeature>());

            Add(systemFactory.Create<HealthFeature>());
            Add(systemFactory.Create<DeathFeature>());

            Add(systemFactory.Create<RewardFeature>());

            Add(systemFactory.Create<GameExchequerFeature>());

            Add(systemFactory.Create<EffectFeature>());

            Add(systemFactory.Create<AnimatorFeature>());

            Add(systemFactory.Create<CooldownFeature>());
            Add(systemFactory.Create<DurationFeature>());

            Add(systemFactory.Create<ProcessDestructedFeature>());
        }

        public override void Execute()
        {
            if (IsPaused())
                return;

            base.Execute();
        }

        public override void Cleanup()
        {
            if (IsPaused())
                return;

            base.Cleanup();
        }

        private bool IsPaused()
        {
            var session = _gameSessions.GetSingleEntity();
            return session != null && session.isPause;
        }
    }
}
