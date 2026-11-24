using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    /// <summary>
    /// Default level map: a start node, three directions and a final node they converge to.
    /// Every node points to the same level until real levels exist.
    /// </summary>
    public static class LevelCatalogDefaults
    {
        public const string Start = "start";
        public const string North = "north";
        public const string Center = "center";
        public const string South = "south";
        public const string Final = "final";

        public static LevelNodeDefinition[] CreateNodes(LevelConfig level) => new[]
        {
            new LevelNodeDefinition(Start, "Start", level, new Vector2(280f, 760f)),
            new LevelNodeDefinition(North, "North", level, new Vector2(820f, 380f), false, Start),
            new LevelNodeDefinition(Center, "Center", level, new Vector2(940f, 700f), false, Start),
            new LevelNodeDefinition(South, "South", level, new Vector2(780f, 1000f), false, Start),
            new LevelNodeDefinition(Final, "Final", level, new Vector2(1600f, 640f), false, North, Center, South),
        };
    }
}
