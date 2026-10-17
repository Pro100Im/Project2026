using System;

namespace Code.Game.Common.Time
{
    public class UnityTimeService : ITimeService
    {
        public float DeltaTime => UnityEngine.Time.deltaTime;

        public float GameDeltaTime => !_paused ? UnityEngine.Time.deltaTime : 0;

        public float TownDeltaTime => UnityEngine.Time.deltaTime;

        public DateTime UtcNow => DateTime.UtcNow;

        public void StopTime() => _paused = true;
        public void StartTime() => _paused = false;

        private bool _paused;
    }
}