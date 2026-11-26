using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "Game/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        public const string AssetPath = "Assets/Configs/Game/Levels/LevelCatalog.asset";

        [Tooltip("World map size in UI pixels. Node positions are in this space, origin at the top-left.")]
        [field: SerializeField] public Vector2 MapSize { get; private set; } = new(2000f, 1200f);

        [Tooltip("World map art. Empty = parchment placeholder.")]
        [field: SerializeField] public Sprite MapBackground { get; private set; }

        [Tooltip("Clouds around the revealed part of the map.")]
        [field: SerializeField] public CloudVeilSettings Veil { get; private set; } = new();

        [SerializeField] private LevelNodeDefinition[] _nodes = Array.Empty<LevelNodeDefinition>();

        public int Count => _nodes?.Length ?? 0;

        public LevelNodeDefinition Get(int index) => _nodes[index];

        public LevelNodeDefinition Start
        {
            get
            {
                for (var i = 0; i < Count; i++)
                {
                    var node = _nodes[i];

                    if (node != null && node.IsRoot)
                        return node;
                }

                return Count > 0 ? _nodes[0] : null;
            }
        }

        public bool TryGetNode(string id, out LevelNodeDefinition node)
        {
            if (!string.IsNullOrEmpty(id))
            {
                for (var i = 0; i < Count; i++)
                {
                    var candidate = _nodes[i];

                    if (candidate == null || candidate.Id != id)
                        continue;

                    node = candidate;
                    return true;
                }
            }

            node = null;
            return false;
        }

        public LevelNodeDefinition ResolveNode(string id) =>
            TryGetNode(id, out var node) ? node : Start;

#if UNITY_EDITOR
        public void SetNodes(LevelNodeDefinition[] nodes) => _nodes = nodes ?? Array.Empty<LevelNodeDefinition>();
#endif
    }
}
