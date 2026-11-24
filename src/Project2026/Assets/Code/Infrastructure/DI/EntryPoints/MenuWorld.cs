using Code.Infrastructure.Systems;
using Code.Meta.Features.LevelMap;
using Entitas;
using System;
using System.Collections.Generic;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class MenuWorld : IInitializable, ITickable, IDisposable
    {
        private readonly ISystemFactory _systems;

        private LevelMapFeature _levelMapFeature;
        private readonly List<MetaEntity> _metaBuffer = new(16);
        private readonly List<InputEntity> _inputBuffer = new(4);

        public MenuWorld(ISystemFactory systems)
        {
            _systems = systems;
        }

        public void Initialize()
        {
            _levelMapFeature = _systems.Create<LevelMapFeature>();
            _levelMapFeature.ActivateReactiveSystems();
            _levelMapFeature.Initialize();
        }

        public void Tick()
        {
            _levelMapFeature?.Execute();
            _levelMapFeature?.Cleanup();
        }

        public void Dispose()
        {
            if (_levelMapFeature == null)
                return;

            _levelMapFeature.DeactivateReactiveSystems();
            _levelMapFeature.ClearReactiveSystems();
            _levelMapFeature.TearDown();
            _levelMapFeature = null;

            // The game scene may already be running next to us: destroy only level map entities,
            // never reset the whole meta context.
            DestroyAll(Contexts.sharedInstance.meta.GetGroup(MetaMatcher.AnyOf(MetaMatcher.LevelNode, MetaMatcher.LevelMapMenu)));
            DestroyAll(Contexts.sharedInstance.input.GetGroup(InputMatcher.AnyOf(
                InputMatcher.LevelMapMenuOpenRequested,
                InputMatcher.LevelMapMenuCloseRequested,
                InputMatcher.LevelNodeChooseRequested)));
        }

        private void DestroyAll(IGroup<MetaEntity> group)
        {
            var entities = group.GetEntities(_metaBuffer);

            for (var i = 0; i < entities.Count; i++)
                entities[i].Destroy();
        }

        private void DestroyAll(IGroup<InputEntity> group)
        {
            var entities = group.GetEntities(_inputBuffer);

            for (var i = 0; i < entities.Count; i++)
                entities[i].Destroy();
        }
    }
}
