using Code.Game.StaticData.Research;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "Town/Research Catalog")]
    public class ResearchCatalog : ScriptableObject
    {
        public const string AssetPath = "Assets/Configs/Research/ResearchCatalog.asset";

        [field: SerializeField] public float NodeSpacing { get; private set; } = 108f;
        [field: SerializeField] public ResearchNodeDefinition[] Nodes { get; private set; }

        [SerializeField] private EntityConfig _veteranKnight;
        [SerializeField] private EntityConfig _eliteKnight;
        [SerializeField] private Sprite _knightIcon;
        [SerializeField] private Sprite _blockIcon;
        [SerializeField] private Sprite _goldIcon;
        [SerializeField] private Sprite _fireIcon;
        [SerializeField] private Sprite _defenseIcon;

        private void OnEnable()
        {
            if (Nodes == null || Nodes.Length == 0)
                ApplyDefaults();
            else
            {
                AssignBranchIcons();
                WireKnightConfigs();
            }
        }

        public void ApplyDefaults()
        {
            NodeSpacing = 108f;
            Nodes = ResearchCatalogDefaults.CreateNodes();
            AssignBranchIcons();
            WireKnightConfigs();
        }

        private void AssignBranchIcons()
        {
            if (Nodes == null)
                return;

            for (var i = 0; i < Nodes.Length; i++)
            {
                var node = Nodes[i];

                if (node == null)
                    continue;

                node.AssignIconIfEmpty(BranchIcon(node.Id));
            }
        }

        private Sprite BranchIcon(ResearchId id)
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
                default:
                    return null;
            }
        }

        private void WireKnightConfigs()
        {
            if (Nodes == null)
                return;

            for (var i = 0; i < Nodes.Length; i++)
            {
                if (Nodes[i].Effect is not KnightUnlockResearchEffect unlock)
                    continue;

                unlock.Config = unlock.Priority <= 1 ? _veteranKnight : _eliteKnight;
            }
        }
    }
}
