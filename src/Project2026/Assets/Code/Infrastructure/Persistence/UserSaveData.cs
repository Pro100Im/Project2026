using Code.Game.Features.Town;
using System.Collections.Generic;

namespace Code.Infrastructure.Persistence
{
    public class UserSaveData
    {
        public const int CurrentVersion = 2;

        public int Version = CurrentVersion;
        public string UserId;
        public TownProgressData Town = new();
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
