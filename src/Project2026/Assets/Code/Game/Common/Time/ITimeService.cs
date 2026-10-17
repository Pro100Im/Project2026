using System;

namespace Code.Game.Common.Time
{
    public interface ITimeService
    {
        float DeltaTime { get; }
        float GameDeltaTime { get; }
        float TownDeltaTime { get; }
        DateTime UtcNow { get; }
        void StopTime();
        void StartTime();
    }
}