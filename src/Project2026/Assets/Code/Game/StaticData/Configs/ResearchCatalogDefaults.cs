using Code.Game.Features.Town;
using Code.Game.StaticData.Research;

namespace Code.Game.StaticData.Configs
{
    public static class ResearchCatalogDefaults
    {
        public static ResearchNodeDefinition[] CreateNodes() => new ResearchNodeDefinition[]
        {
            Node(ResearchId.Knights, "Knights", "Knights are already trained.", 0, ResearchLinkDirection.West, startingLevel: 1),
            Node(ResearchId.VeteranKnight, "Veteran", "Unlocks a stronger knight. Requires Barracks.", 50, ResearchLinkDirection.West, new KnightUnlockResearchEffect { Priority = 1 }, ResearchId.Knights, true, BuildingType.Barracks, 1),
            Node(ResearchId.EliteKnight, "Elite", "Unlocks the strongest knight.", 150, ResearchLinkDirection.West, new KnightUnlockResearchEffect { Priority = 2 }, ResearchId.VeteranKnight),

            Node(ResearchId.Block1, "Block I", "Knights gain 10% block chance.", 15, ResearchLinkDirection.SouthWest, new KnightBlockResearchEffect { Value = 0.1f }, ResearchId.Knights),
            Node(ResearchId.Block2, "Block II", "Knights gain 10% block chance.", 30, ResearchLinkDirection.SouthWest, new KnightBlockResearchEffect { Value = 0.1f }, ResearchId.Block1),
            Node(ResearchId.Block3, "Block III", "Knights gain 10% block chance.", 45, ResearchLinkDirection.SouthWest, new KnightBlockResearchEffect { Value = 0.1f }, ResearchId.Block2),
            Node(ResearchId.Block4, "Block IV", "Knights gain 10% block chance.", 60, ResearchLinkDirection.SouthWest, new KnightBlockResearchEffect { Value = 0.1f }, ResearchId.Block3),
            Node(ResearchId.Block5, "Block V", "Knights gain 10% block chance.", 80, ResearchLinkDirection.SouthWest, new KnightBlockResearchEffect { Value = 0.1f }, ResearchId.Block4),

            Node(ResearchId.Gold1, "Gold I", "Gold income +20%.", 10, ResearchLinkDirection.East, new GoldIncomeResearchEffect { Value = 0.2f }),
            Node(ResearchId.Gold2, "Gold II", "Gold income +20%.", 20, ResearchLinkDirection.East, new GoldIncomeResearchEffect { Value = 0.2f }, ResearchId.Gold1),
            Node(ResearchId.Gold3, "Gold III", "Gold income +20%.", 40, ResearchLinkDirection.East, new GoldIncomeResearchEffect { Value = 0.2f }, ResearchId.Gold2),
            Node(ResearchId.Gold4, "Gold IV", "Gold income +20%.", 80, ResearchLinkDirection.East, new GoldIncomeResearchEffect { Value = 0.2f }, ResearchId.Gold3),
            Node(ResearchId.Gold5, "Gold V", "Gold income +20%.", 160, ResearchLinkDirection.East, new GoldIncomeResearchEffect { Value = 0.2f }, ResearchId.Gold4),

            Node(ResearchId.Fire1, "Fire I", "Fire damage +15%.", 10, ResearchLinkDirection.North, new FireDamageResearchEffect { Value = 0.15f }),
            Node(ResearchId.Fire2, "Fire II", "Fire damage +15%.", 20, ResearchLinkDirection.North, new FireDamageResearchEffect { Value = 0.15f }, ResearchId.Fire1),
            Node(ResearchId.Fire3, "Fire III", "Fire damage +15%.", 40, ResearchLinkDirection.North, new FireDamageResearchEffect { Value = 0.15f }, ResearchId.Fire2),
            Node(ResearchId.Fire4, "Fire IV", "Fire damage +15%.", 80, ResearchLinkDirection.North, new FireDamageResearchEffect { Value = 0.15f }, ResearchId.Fire3),
            Node(ResearchId.Fire5, "Fire V", "Fire damage +15%.", 160, ResearchLinkDirection.North, new FireDamageResearchEffect { Value = 0.15f }, ResearchId.Fire4),

            Node(ResearchId.Defense1, "Defense I", "Defenders gain 4 resistance.", 10, ResearchLinkDirection.South, new ResistanceResearchEffect { Value = 4 }),
            Node(ResearchId.Defense2, "Defense II", "Defenders gain 4 resistance.", 20, ResearchLinkDirection.South, new ResistanceResearchEffect { Value = 4 }, ResearchId.Defense1),
            Node(ResearchId.Defense3, "Defense III", "Defenders gain 4 resistance.", 40, ResearchLinkDirection.South, new ResistanceResearchEffect { Value = 4 }, ResearchId.Defense2),
            Node(ResearchId.Defense4, "Defense IV", "Defenders gain 4 resistance.", 80, ResearchLinkDirection.South, new ResistanceResearchEffect { Value = 4 }, ResearchId.Defense3),
            Node(ResearchId.Defense5, "Defense V", "Defenders gain 4 resistance.", 160, ResearchLinkDirection.South, new ResistanceResearchEffect { Value = 4 }, ResearchId.Defense4)
        };

        private static ResearchNodeDefinition Node(
            ResearchId id,
            string title,
            string description,
            int cost,
            ResearchLinkDirection direction,
            ResearchEffect effect = null,
            ResearchId parent = ResearchId.None,
            bool barracks = false,
            BuildingType building = BuildingType.Barracks,
            int buildingLevel = 1,
            int startingLevel = 0) =>
            new(
                id,
                title,
                description,
                cost,
                direction,
                effect,
                parent,
                startingLevel,
                barracks,
                building,
                buildingLevel);
    }
}
