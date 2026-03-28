using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Wave
{
    [Game] public class WaveInProgress : IComponent { }
    [Game] public class WaveStartRequsted : IComponent { }
    [Game] public class WaveEnemiesAlive : IComponent { public int Value; }
    [Game] public class CurrentWaveNumber : IComponent { public int Value; }
    [Game] public class CurrentWaveEnemies : IComponent { public List<EntityConfig> Value; }
}