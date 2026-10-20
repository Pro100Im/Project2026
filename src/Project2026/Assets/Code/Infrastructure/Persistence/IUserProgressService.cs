using Code.Game.Features.Town;

namespace Code.Infrastructure.Persistence
{
    public interface IUserProgressService
    {
        int Gold { get; }
        int Stone { get; }
        int Crystal { get; }
        int Wood { get; }
        int Knowledge { get; }

        bool TryGetBuildingLevel(BuildingType type, out int level);
        bool TryGetResearchLevel(ResearchId id, out int level);
        void SetTownResources(int gold, int stone, int crystal, int wood, int knowledge);
        void SetBuildingLevel(BuildingType type, int level);
        void SetResearchLevel(ResearchId id, int level);
#if UNITY_EDITOR
        void ResetResources();
        void ResetBuildings();
        void ResetResearch();
        void ResetPlayer();
#endif
        void Flush();
    }
}
