using Code.Game.Features.Town;
using Code.Game.StaticData.Research;

namespace Code.Game.StaticData.Configs
{
    public static class ResearchCatalogDefaults
    {
        public static ResearchBranchDefinition[] CreateBranches() => new ResearchBranchDefinition[]
        {
            Branch("Knights", ResearchLinkDirection.West,
                Node(ResearchId.Knights, "Knights", "Unlocks the basic knight. Requires Barracks.", 0, new UnitSlotUnlockResearchEffect { Kind = UnitKind.Knight }, true, BuildingType.Barracks, 1),
                Branch("Block", ResearchLinkDirection.SouthWest,
                    Node(ResearchId.Block1, "Block I", "Knights gain 10% block chance.", 15, new KnightBlockResearchEffect { Value = 0.1f }),
                    Node(ResearchId.Block2, "Block II", "Knights gain 10% block chance.", 30, new KnightBlockResearchEffect { Value = 0.1f }),
                    Node(ResearchId.Block3, "Block III", "Knights gain 10% block chance.", 45, new KnightBlockResearchEffect { Value = 0.1f }),
                    Node(ResearchId.Block4, "Block IV", "Knights gain 10% block chance.", 60, new KnightBlockResearchEffect { Value = 0.1f }),
                    Node(ResearchId.Block5, "Block V", "Knights gain 10% block chance.", 80, new KnightBlockResearchEffect { Value = 0.1f })),
                Branch("Archers", ResearchLinkDirection.NorthWest,
                    Node(ResearchId.Archers, "Archers", "Unlocks archers. Requires Archery.", 10, new UnitSlotUnlockResearchEffect { Kind = UnitKind.Archer }, true, BuildingType.Archery, 1)),
                Node(ResearchId.VeteranKnight, "Veteran", "Unlocks a stronger knight. Requires Barracks.", 50, new KnightUnlockResearchEffect { Priority = 1 }, true, BuildingType.Barracks, 1),
                Node(ResearchId.EliteKnight, "Elite", "Unlocks the strongest knight.", 150, new KnightUnlockResearchEffect { Priority = 2 })),

            Branch("Gold", ResearchLinkDirection.East,
                Node(ResearchId.Gold1, "Gold I", "Gold income +20%.", 10, new GoldIncomeResearchEffect { Value = 0.2f }),
                Node(ResearchId.Gold2, "Gold II", "Gold income +20%.", 20, new GoldIncomeResearchEffect { Value = 0.2f }),
                Node(ResearchId.Gold3, "Gold III", "Gold income +20%.", 40, new GoldIncomeResearchEffect { Value = 0.2f }),
                Node(ResearchId.Gold4, "Gold IV", "Gold income +20%.", 80, new GoldIncomeResearchEffect { Value = 0.2f }),
                Node(ResearchId.Gold5, "Gold V", "Gold income +20%.", 160, new GoldIncomeResearchEffect { Value = 0.2f })),

            Branch("Fire", ResearchLinkDirection.North,
                Node(ResearchId.Fire1, "Fire I", "Fire damage +15%.", 10, new FireDamageResearchEffect { Value = 0.15f }),
                Node(ResearchId.Fire2, "Fire II", "Fire damage +15%.", 20, new FireDamageResearchEffect { Value = 0.15f }),
                Node(ResearchId.Fire3, "Fire III", "Fire damage +15%.", 40, new FireDamageResearchEffect { Value = 0.15f }),
                Node(ResearchId.Fire4, "Fire IV", "Fire damage +15%.", 80, new FireDamageResearchEffect { Value = 0.15f }),
                Node(ResearchId.Fire5, "Fire V", "Fire damage +15%.", 160, new FireDamageResearchEffect { Value = 0.15f })),

            Branch("Defense", ResearchLinkDirection.South,
                Node(ResearchId.Defense1, "Defense I", "Defenders gain 4 resistance.", 10, new ResistanceResearchEffect { Value = 4 }),
                Node(ResearchId.Defense2, "Defense II", "Defenders gain 4 resistance.", 20, new ResistanceResearchEffect { Value = 4 }),
                Node(ResearchId.Defense3, "Defense III", "Defenders gain 4 resistance.", 40, new ResistanceResearchEffect { Value = 4 }),
                Node(ResearchId.Defense4, "Defense IV", "Defenders gain 4 resistance.", 80, new ResistanceResearchEffect { Value = 4 }),
                Node(ResearchId.Defense5, "Defense V", "Defenders gain 4 resistance.", 160, new ResistanceResearchEffect { Value = 4 }))
        };

        private static ResearchBranchDefinition Branch(string name, ResearchLinkDirection direction, params ResearchBranchEntry[] entries) =>
            new(name, direction, entries);

        private static ResearchNodeDefinition Node(
            ResearchId id,
            string title,
            string description,
            int cost,
            ResearchEffect effect = null,
            bool barracks = false,
            BuildingType building = BuildingType.Barracks,
            int buildingLevel = 1,
            int startingLevel = 0) =>
            new(
                id,
                title,
                description,
                cost,
                effect,
                startingLevel,
                barracks,
                building,
                buildingLevel);
    }
}
