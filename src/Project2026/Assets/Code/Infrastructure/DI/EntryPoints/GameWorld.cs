using Code.Game.Common.Entity;
using Code.Game.Common.Time;
using Code.Game.Common.UI.Transition;
using Code.Game.Features;
using Code.Game.Features.Input;
using Code.Infrastructure.Identifiers;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Code.Infrastructure.View.Pool;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class GameWorld : ITickable, IInitializable, IDisposable
    {
        private readonly ISystemFactory _systems;
        private readonly TransitionScreen _transitionScreen;
        private readonly IEntityViewPool _viewPool;
        private readonly ITimeService _timeService;
        private readonly InputFeature _inputFeature;

        private GameTickFeature _gameTickFeature;
        private readonly List<GameEntity> _viewReleaseBuffer = new(128);

        public GameWorld(
            TransitionScreen transitionScreen,
            ISystemFactory systems,
            InputFeature inputFeature,
            IEntityViewPool viewPool,
            ITimeService timeService)
        {
            _systems = systems;
            _transitionScreen = transitionScreen;
            _inputFeature = inputFeature;
            _viewPool = viewPool;
            _timeService = timeService;
        }

        public void Initialize()
        {
            _timeService.StartTime();
            CreateGameSession();
        }

        private void CreateGameSession()
        {
            var entity = CreateGameEntity.Empty();
            entity.isGameSession = true;

            _gameTickFeature = _systems.Create<GameTickFeature>();
            _gameTickFeature.ActivateReactiveSystems();
            _inputFeature.ActivateReactiveSystems();

            _gameTickFeature.Initialize();

            _transitionScreen.Hide().Forget();
        }

        public void Tick()
        {
            _gameTickFeature?.Execute();
            _gameTickFeature?.Cleanup();
        }

        public void Dispose()
        {
            if (_gameTickFeature == null)
                return;

            _gameTickFeature.DeactivateReactiveSystems();
            _inputFeature.DeactivateReactiveSystems();

            ReleaseAllBoundViews();

            _gameTickFeature.ClearReactiveSystems();
            _gameTickFeature.TearDown();

            _inputFeature.ClearReactiveSystems();
            _inputFeature.TearDown();

            Contexts.sharedInstance.game.Reset();
            Contexts.sharedInstance.meta.Reset();
            Contexts.sharedInstance.input.Reset();
            Contexts.sharedInstance.network.Reset();

            EntityIdentifier.Reset();
            _timeService.StartTime();
            _viewPool.Clear();

            _gameTickFeature = null;
        }

        private void ReleaseAllBoundViews()
        {
            var entities = Contexts.sharedInstance.game.GetGroup(GameMatcher.View).GetEntities(_viewReleaseBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                if (!entity.hasView)
                    continue;

                entity.view.Value.ReleaseEntity();
            }

            var metaViews = UnityEngine.Object.FindObjectsByType<MetaEntityBehaviour>(FindObjectsInactive.Include);

            for (var i = 0; i < metaViews.Length; i++)
            {
                if (metaViews[i].Entity != null)
                    metaViews[i].ReleaseEntity();
            }
        }
    }
}
