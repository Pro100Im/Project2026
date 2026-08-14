using System;
using UnityEngine;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class GlobalWorld : ITickable, IInitializable, IDisposable
    {
        private readonly GameObject _eventSystem;
        private readonly GameObject _audioListener;

        public GlobalWorld(GameObject eventSystem, GameObject audioListener) 
        {
            _eventSystem = eventSystem;
            _audioListener = audioListener;
        }

        public void Initialize()
        {
            var eventSystemInstance = GameObject.Instantiate(_eventSystem);
            var audioListenerInstance = GameObject.Instantiate(_audioListener);

            GameObject.DontDestroyOnLoad(eventSystemInstance);
            GameObject.DontDestroyOnLoad(audioListenerInstance);
        }

        public void Tick()
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}