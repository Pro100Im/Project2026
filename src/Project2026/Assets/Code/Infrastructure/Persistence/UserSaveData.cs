using Code.Game.Features.Town;
using System.Collections.Generic;

namespace Code.Infrastructure.Persistence
{
    public class UserSaveData
    {
        public const int CurrentVersion = 1;

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
    }

    public class TownBuildingProgressData
    {
        public BuildingType Type;
        public int Level;
    }
}
