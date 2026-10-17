using Code.Game.Common.Entity;
using Code.Game.Common.Time;
using Code.Game.Features.Town;
using Code.Infrastructure.Identifiers;
using Code.Infrastructure.Persistence;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Code.Infrastructure.View.Pool;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class TownWorld : ITickable, IInitializable, IDisposable
    {
        private readonly ISystemFactory _systems;
        private readonly IEntityViewPool _viewPool;
        private readonly ITimeService _timeService;
        private readonly IUserProgressService _progress;

        private TownFeature _townFeature;
        private readonly List<GameEntity> _viewReleaseBuffer = new(128);

        public TownWorld(ISystemFactory systems, IEntityViewPool viewPool, ITimeService timeService, IUserProgressService progress)
        {
            _systems = systems;
            _viewPool = viewPool;
            _timeService = timeService;
            _progress = progress;
        }

        public void Initialize()
        {
            _timeService.StartTime();
            CreateGameSession();
        }

        private void CreateGameSession()
        {
            _townFeature = _systems.Create<TownFeature>();
            _townFeature.ActivateReactiveSystems();

            _townFeature.Initialize();
        }

        public void Tick()
        {
            _townFeature?.Execute();
            _townFeature?.Cleanup();
        }

        public void Dispose()
        {
            _progress.Flush();

            if (_townFeature == null)
                return;

            _townFeature.DeactivateReactiveSystems();

            ReleaseAllBoundViews();

            _townFeature.ClearReactiveSystems();
            _townFeature.TearDown();

            Contexts.sharedInstance.town.Reset();
            Contexts.sharedInstance.meta.Reset();

            EntityIdentifier.Reset();

            _timeService.StartTime();
            _viewPool.Clear();

            _townFeature = null;
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

            var townViews = UnityEngine.Object.FindObjectsByType<TownEntityBehaviour>(FindObjectsInactive.Include);

            for (var i = 0; i < townViews.Length; i++)
            {
                if (townViews[i].Entity != null)
                    townViews[i].ReleaseEntity();
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