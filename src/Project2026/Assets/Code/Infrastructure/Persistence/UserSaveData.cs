using Code.Game.Features.Town;
using System.Collections.Generic;

namespace Code.Infrastructure.Persistence
{
    public class UserSaveData
    {
        public const int CurrentVersion = 5;

        public int Version = CurrentVersion;
        public string UserId;
        public TownProgressData Town = new();
        public CampaignProgressData Campaign = new();
    }

    public class TownProgressData
    {
        public int Gold;
        public int Stone;
        public int Crystal;
        public int Wood;
        public int Knowledge;
        public List<TownBuildingProgressData> Buildings = new(8);
        public List<ResearchProgressData> Research = new(8);
        public List<UnitSlotProgressData> UnitSlots = new(3);

#if UNITY_EDITOR
        public void ClearResources()
        {
            Gold = 0;
            Stone = 0;
            Crystal = 0;
            Wood = 0;
            Knowledge = 0;
        }

        public void ClearBuildings()
        {
            Buildings ??= new List<TownBuildingProgressData>(8);
            Buildings.Clear();
        }

        public void ClearResearch()
        {
            Research ??= new List<ResearchProgressData>(8);
            Research.Clear();
        }
#endif
    }

    public class CampaignProgressData
    {
        public List<LevelProgressData> Levels = new(8);

        // The state each map node had when the player last saw it, so map reveal animations play once.
        public List<LevelSeenData> Seen = new(8);

        // Version 3 stored only completion. Migrated into Levels on load.
        public List<string> CompletedLevels;
    }

    public class LevelProgressData
    {
        public string Id;
        public int Stars;
    }

    public class LevelSeenData
    {
        public string Id;
        public int State;
    }

    public class TownBuildingProgressData
    {
        public BuildingType Type;
        public int Level;
    }

    public class ResearchProgressData
    {
        public ResearchId Id;
        public int Level;
    }

    public class UnitSlotProgressData
    {
        public int Slot;
        public UnitKind Kind;
    }
}
