using Code.Game.Features.Town;
using Code.Game.StaticData.Research;
using Code.Infrastructure.Persistence;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [Serializable]
    public class ResearchNodeDefinition : ResearchBranchEntry
    {
        [field: SerializeField] public ResearchId Id { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int MaxLevel { get; private set; } = 1;
        [field: SerializeField] public int StartingLevel { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }

        // Kept so the flat catalog can be migrated. Layout is bound from the branch.
        [field: SerializeField, HideInInspector] public ResearchId Parent { get; private set; }
        [field: SerializeField, HideInInspector] public ResearchLinkDirection Direction { get; private set; }

        [field: SerializeField] public BuildingType RequiredBuilding { get; private set; }
        [field: SerializeField] public int RequiredBuildingLevel { get; private set; }
        [field: SerializeField] public bool HasBuildingRequirement { get; private set; }

        [SerializeReference] public ResearchEffect Effect;

        public ResearchNodeDefinition()
        {
            MaxLevel = 1;
        }

        public ResearchNodeDefinition(
            ResearchId id,
            string title,
            string description,
            int cost,
            ResearchEffect effect = null,
            int startingLevel = 0,
            bool hasBuildingRequirement = false,
            BuildingType requiredBuilding = BuildingType.Houses,
            int requiredBuildingLevel = 0)
        {
            Id = id;
            Title = title;
            Description = description;
            Cost = cost;
            Effect = effect;
            StartingLevel = startingLevel;
            MaxLevel = 1;
            HasBuildingRequirement = hasBuildingRequirement;
            RequiredBuilding = requiredBuilding;
            RequiredBuildingLevel = requiredBuildingLevel;
        }

        public void Bind(ResearchId parent, ResearchLinkDirection direction)
        {
            Parent = parent;
            Direction = direction;
        }

        public int ResolveLevel(IUserProgressService progress)
        {
            var level = progress.TryGetResearchLevel(Id, out var saved) ? saved : StartingLevel;

            if (level < 0)
                level = 0;

            if (level > MaxLevel)
                level = MaxLevel;

            return level;
        }

        public void ApplyTown(TownEntity node, int level)
        {
            node.isResearchNode = true;
            node.AddResearchId(Id);
            node.AddResearchLevel(level);
            node.AddResearchMaxLevel(MaxLevel);
            node.AddResearchStartingLevel(StartingLevel);
            node.AddResearchCost(Cost);
            node.AddResearchDirection(Direction);
            node.AddResearchTitle(Title);
            node.AddResearchDescription(Description ?? string.Empty);

            if (Icon != null)
                node.AddResearchIcon(Icon);

            if (Parent != ResearchId.None)
                node.AddResearchParent(Parent);

            if (HasBuildingRequirement)
            {
                node.AddResearchRequiredBuilding(RequiredBuilding);
                node.AddResearchRequiredBuildingLevel(RequiredBuildingLevel);
            }

            Effect?.ApplyTown(node);
        }

        public void ApplyGame(GameEntity node, int level)
        {
            node.isResearchNode = true;
            node.AddResearchId(Id);
            node.AddResearchLevel(level);
            node.AddResearchMaxLevel(MaxLevel);
            node.AddResearchStartingLevel(StartingLevel);
            node.AddResearchCost(Cost);

            if (Parent != ResearchId.None)
                node.AddResearchParent(Parent);

            if (HasBuildingRequirement)
            {
                node.AddResearchRequiredBuilding(RequiredBuilding);
                node.AddResearchRequiredBuildingLevel(RequiredBuildingLevel);
            }

            Effect?.ApplyGame(node);
        }

        public void AssignIconIfEmpty(Sprite icon)
        {
            if (Icon == null && icon != null)
                Icon = icon;
        }
    }
}
