using Code.Infrastructure.Persistence;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    /// <summary>
    /// A node on the level map. Several nodes may point to the same <see cref="LevelConfig"/>,
    /// progress is tracked per node <see cref="Id"/>.
    /// </summary>
    [Serializable]
    public class LevelNodeDefinition
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public LevelConfig Level { get; private set; }

        [Tooltip("Ids of nodes that unlock this one. Empty = starting node.")]
        [field: SerializeField] public string[] Requires { get; private set; } = Array.Empty<string>();

        [Tooltip("Off: any of Requires completed unlocks the node. On: all of them are needed.")]
        [field: SerializeField] public bool RequireAll { get; private set; }

        [field: SerializeField] public Vector2 Position { get; private set; }

        public bool IsRoot => Requires == null || Requires.Length == 0;

        public LevelNodeDefinition() { }

        public LevelNodeDefinition(string id, string title, LevelConfig level, Vector2 position,
            bool requireAll = false, params string[] requires)
        {
            Id = id;
            Title = title;
            Level = level;
            Position = position;
            RequireAll = requireAll;
            Requires = requires ?? Array.Empty<string>();
        }

        public void ApplyMeta(MetaEntity node, IUserProgressService progress)
        {
            node.isLevelNode = true;
            node.AddLevelNodeId(Id);
            node.AddLevelNodeRequires(Requires ?? Array.Empty<string>());
            node.AddLevelNodePosition(Position);
            node.AddLevelNodeTitle(ResolveTitle());
            node.isLevelNodeRequireAll = RequireAll;
            node.isLevelNodeCompleted = progress.TryGetLevelStars(Id, out var stars);
            node.AddLevelNodeStars(stars);

            var icon = Level != null ? Level.Icon : null;

            if (icon != null)
                node.AddLevelNodeIcon(icon);
        }

        private string ResolveTitle()
        {
            if (!string.IsNullOrEmpty(Title))
                return Title;

            if (Level != null && !string.IsNullOrEmpty(Level.Title))
                return Level.Title;

            return Id;
        }
    }
}
