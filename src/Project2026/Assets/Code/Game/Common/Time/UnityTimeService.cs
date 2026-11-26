using System;

namespace Code.Game.Common.Time
{
    public class UnityTimeService : ITimeService
    {
        public float DeltaTime => _simulationStep > 0f ? _simulationStep : UnityEngine.Time.deltaTime;

        public float GameDeltaTime => !_paused ? DeltaTime : 0;

        public float TownDeltaTime => UnityEngine.Time.deltaTime;

        public float SimulationAlpha { get; set; } = 1f;

        public DateTime UtcNow => DateTime.UtcNow;

        public void StopTime() => _paused = true;
        public void StartTime() => _paused = false;

        public void BeginSimulationStep(float step) => _simulationStep = step;
        public void EndSimulationStep() => _simulationStep = 0f;

        private bool _paused;
        private float _simulationStep;
    }
}
