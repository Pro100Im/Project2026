using System;

namespace Code.Game.Common.Time
{
    public interface ITimeService
    {
        /// <summary>Frame delta, or the fixed step while a simulation step is being executed.</summary>
        float DeltaTime { get; }
        float GameDeltaTime { get; }
        float TownDeltaTime { get; }

        /// <summary>How far the current frame is between the last two simulation steps, 0..1.</summary>
        float SimulationAlpha { get; set; }

        DateTime UtcNow { get; }
        void StopTime();
        void StartTime();
        void BeginSimulationStep(float step);
        void EndSimulationStep();
    }
}
