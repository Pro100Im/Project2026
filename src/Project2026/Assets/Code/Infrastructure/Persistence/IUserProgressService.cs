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
        bool TryGetUnitSlot(int slot, out UnitKind kind);
        void SetTownResources(int gold, int stone, int crystal, int wood, int knowledge);
        void SetBuildingLevel(BuildingType type, int level);
        void SetResearchLevel(ResearchId id, int level);
        void SetUnitSlot(int slot, UnitKind kind);
        void ClearUnitSlot(int slot);
        bool IsLevelCompleted(string levelNodeId);
        bool TryGetLevelStars(string levelNodeId, out int stars);
        void CompleteLevel(string levelNodeId, int stars);
        bool TryGetSeenLevelState(string levelNodeId, out LevelNodeState state);
        void SetSeenLevelState(string levelNodeId, LevelNodeState state);
#if UNITY_EDITOR
        void ResetResources();
        void ResetBuildings();
        void ResetResearch();
        void ResetLevels();
        void ResetSeenLevels();
        void ResetPlayer();
#endif
        void Flush();
    }
}
