using Code.Game.StaticData.Research;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "Town/Research Catalog")]
    public class ResearchCatalog : ScriptableObject
    {
        public const string AssetPath = "Assets/Configs/Research/ResearchCatalog.asset";

        [field: SerializeField] public float NodeSpacing { get; private set; } = 108f;
        [field: SerializeField] public ResearchBranchDefinition[] Branches { get; private set; }

        [field: SerializeField, HideInInspector] public ResearchNodeDefinition[] Nodes { get; private set; }

        [SerializeField] private EntityConfig _veteranKnight;
        [SerializeField] private EntityConfig _eliteKnight;
        [SerializeField] private Sprite _knightIcon;
        [SerializeField] private Sprite _blockIcon;
        [SerializeField] private Sprite _goldIcon;
        [SerializeField] private Sprite _fireIcon;
        [SerializeField] private Sprite _defenseIcon;
        [SerializeField] private Sprite _archerIcon;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && HasLegacyNodes && !HasBranches)
            {
                UnityEditor.EditorApplication.delayCall -= MigrateLegacyNodes;
                UnityEditor.EditorApplication.delayCall += MigrateLegacyNodes;
            }
#endif
            if (HasBranches || HasLegacyNodes)
            {
                PrepareNodes();
                return;
            }

            ApplyDefaults();
        }

        public void ApplyDefaults()
        {
            NodeSpacing = 108f;
            Branches = ResearchCatalogDefaults.CreateBranches();
            Nodes = System.Array.Empty<ResearchNodeDefinition>();
            PrepareNodes();
        }

        public void Collect(List<ResearchNodeDefinition> result)
        {
            result.Clear();

            if (HasBranches)
            {
                for (var i = 0; i < Branches.Length; i++)
                    CollectBranch(Branches[i], ResearchId.None, result);

                return;
            }

            if (!HasLegacyNodes)
                return;

            for (var i = 0; i < Nodes.Length; i++)
            {
                if (Nodes[i] != null)
                    result.Add(Nodes[i]);
            }
        }

        public void MigrateLegacyNodes()
        {
#if UNITY_EDITOR
            if (Application.isPlaying || HasBranches || !HasLegacyNodes)
                return;

            Branches = ResearchCatalogMigration.FromFlat(Nodes);
            Nodes = System.Array.Empty<ResearchNodeDefinition>();
            PrepareNodes();
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private bool HasBranches => Branches != null && Branches.Length > 0;

        private bool HasLegacyNodes => Nodes != null && Nodes.Length > 0;

        private static void CollectBranch(
            ResearchBranchDefinition branch,
            ResearchId anchor,
            List<ResearchNodeDefinition> result)
        {
            if (branch?.Entries == null)
                return;

            var previous = anchor;

            for (var i = 0; i < branch.Entries.Length; i++)
            {
                switch (branch.Entries[i])
                {
                    case ResearchNodeDefinition node:
                        node.Bind(previous, branch.Direction);
                        result.Add(node);
                        previous = node.Id;
                        break;
                    case ResearchBranchDefinition nested:
                        CollectBranch(nested, previous, result);
                        break;
                }
            }
        }

        private void PrepareNodes()
        {
            if (HasBranches)
            {
                for (var i = 0; i < Branches.Length; i++)
                    PrepareBranch(Branches[i]);

                return;
            }

            if (!HasLegacyNodes)
                return;

            for (var i = 0; i < Nodes.Length; i++)
                PrepareNode(Nodes[i]);
        }

        private void PrepareBranch(ResearchBranchDefinition branch)
        {
            if (branch?.Entries == null)
                return;

            for (var i = 0; i < branch.Entries.Length; i++)
            {
                switch (branch.Entries[i])
                {
                    case ResearchNodeDefinition node:
                        PrepareNode(node);
                        break;
                    case ResearchBranchDefinition nested:
                        PrepareBranch(nested);
                        break;
                }
            }
        }

        private void PrepareNode(ResearchNodeDefinition node)
        {
            if (node == null)
                return;

            node.AssignIconIfEmpty(IconFor(node.Id));

            if (node.Effect is not KnightUnlockResearchEffect unlock)
                return;

            unlock.Config = unlock.Priority <= 1 ? _veteranKnight : _eliteKnight;
        }

        private Sprite IconFor(ResearchId id)
        {
            switch (id)
            {
                case ResearchId.Knights:
                case ResearchId.VeteranKnight:
                case ResearchId.EliteKnight:
                    return _knightIcon;
                case ResearchId.Block1:
                case ResearchId.Block2:
                case ResearchId.Block3:
                case ResearchId.Block4:
                case ResearchId.Block5:
                    return _blockIcon;
                case ResearchId.Gold1:
                case ResearchId.Gold2:
                case ResearchId.Gold3:
                case ResearchId.Gold4:
                case ResearchId.Gold5:
                    return _goldIcon;
                case ResearchId.Fire1:
                case ResearchId.Fire2:
                case ResearchId.Fire3:
                case ResearchId.Fire4:
                case ResearchId.Fire5:
                    return _fireIcon;
                case ResearchId.Defense1:
                case ResearchId.Defense2:
                case ResearchId.Defense3:
                case ResearchId.Defense4:
                case ResearchId.Defense5:
                    return _defenseIcon;
                case ResearchId.Archers:
                    return _archerIcon;
                default:
                    return null;
            }
        }
    }
}
