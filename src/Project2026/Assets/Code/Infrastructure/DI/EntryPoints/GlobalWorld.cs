using Code.Game.Features.Input;
using Code.Infrastructure.Systems;
using System;
using UnityEngine;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class GlobalWorld : ITickable, ILateTickable, IInitializable, IDisposable
    {
        private readonly GameObject _eventSystem;
        private readonly GameObject _audioListener;
        private readonly ISystemFactory _systems;

        private InputFeature _inputFeature;

        public GlobalWorld(GameObject eventSystem, GameObject audioListener, ISystemFactory systems)
        {
            _eventSystem = eventSystem;
            _audioListener = audioListener;
            _systems = systems;
        }

        public void Initialize()
        {
            var eventSystemInstance = GameObject.Instantiate(_eventSystem);
            var audioListenerInstance = GameObject.Instantiate(_audioListener);

            GameObject.DontDestroyOnLoad(eventSystemInstance);
            GameObject.DontDestroyOnLoad(audioListenerInstance);

            _inputFeature = _systems.Create<InputFeature>();
            _inputFeature.ActivateReactiveSystems();
            _inputFeature.Initialize();
        }

        public void Tick()
        {
            _inputFeature?.Execute();
        }

        public void LateTick()
        {
            _inputFeature?.Cleanup();
        }

        public void Dispose()
        {
            if (_inputFeature == null)
                return;

            _inputFeature.DeactivateReactiveSystems();
            _inputFeature.ClearReactiveSystems();
            _inputFeature.TearDown();
            _inputFeature = null;
        }
    }
}
