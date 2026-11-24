using Code.Game.StaticData.Configs;
using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Code.Game.Features.LevelLoad
{
    [Input, Unique] public class SelectedLevel : IComponent { public string Id; }

    [Game] public class ActiveLevel : IComponent { public LevelConfig Value; }
    [Game] public class ActiveLevelNode : IComponent { public string Value; }
    [Game] public class LevelStars : IComponent { public int Value; }
    [Game] public class LevelSpawnRequested : IComponent { }
    [Game] public class LevelLoaded : IComponent { }
    [Game] public class WaveSchedule : IComponent { public WavesConfig Value; }
}
