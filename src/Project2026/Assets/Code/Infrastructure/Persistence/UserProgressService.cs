using Code.Game.Features.Town;
using Code.Game.StaticData.Configs;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Code.Infrastructure.Persistence
{
    public class UserProgressService : IUserService, IUserProgressService, ITickable, IDisposable
    {
        public const string SaveKey = "user";
        private const float SaveIntervalSeconds = 5f;

        private readonly IStorageService _storage;

        private UserSaveData _save;
        private ProgressSaveHook _hook;
        private float _dirtyElapsed;
        private bool _dirty;
        private bool _disposed;

        public UserProgressService(IStorageService storage)
        {
            _storage = storage;
            LoadOrCreate();
            _hook = CreateHook();
        }

        public string UserId => _save.UserId;
        public int Gold => _save.Town.Gold;
        public int Stone => _save.Town.Stone;
        public int Crystal => _save.Town.Crystal;
        public int Wood => _save.Town.Wood;
        public int Knowledge => _save.Town.Knowledge;

        public void Tick() => Advance(Time.unscaledDeltaTime);

        private void Advance(float unscaledDeltaTime)
        {
            if (_disposed || !_dirty || unscaledDeltaTime <= 0f)
                return;

            _dirtyElapsed += unscaledDeltaTime;

            if (_dirtyElapsed < SaveIntervalSeconds)
                return;

            Flush();
        }

        public bool TryGetBuildingLevel(BuildingType type, out int level)
        {
            var buildings = _save.Town.Buildings;

            for (var i = 0; i < buildings.Count; i++)
            {
                if (buildings[i].Type != type)
                    continue;

                level = buildings[i].Level;
                return true;
            }

            level = 0;
            return false;
        }

        public bool TryGetResearchLevel(ResearchId id, out int level)
        {
            var research = _save.Town.Research;

            for (var i = 0; i < research.Count; i++)
            {
                if (research[i].Id != id)
                    continue;

                level = research[i].Level;
                return true;
            }

            level = 0;
            return false;
        }

        public bool TryGetUnitSlot(int slot, out UnitKind kind)
        {
            var slots = _save.Town.UnitSlots;

            if (slots != null)
            {
                for (var i = 0; i < slots.Count; i++)
                {
                    if (slots[i].Slot != slot)
                        continue;

                    kind = slots[i].Kind;
                    return true;
                }
            }

            kind = default;
            return false;
        }

        public void SetTownResources(int gold, int stone, int crystal, int wood, int knowledge)
        {
            var town = _save.Town;

            if (town.Gold == gold &&
                town.Stone == stone &&
                town.Crystal == crystal &&
                town.Wood == wood &&
                town.Knowledge == knowledge)
                return;

            town.Gold = gold;
            town.Stone = stone;
            town.Crystal = crystal;
            town.Wood = wood;
            town.Knowledge = knowledge;
            MarkDirty();
        }

        public void SetBuildingLevel(BuildingType type, int level)
        {
            if (level < 0)
                level = 0;

            var buildings = _save.Town.Buildings;

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];

                if (building.Type != type)
                    continue;

                if (building.Level == level)
                    return;

                building.Level = level;
                MarkDirty();
                return;
            }

            buildings.Add(new TownBuildingProgressData
            {
                Type = type,
                Level = level
            });
            MarkDirty();
        }

        public void SetResearchLevel(ResearchId id, int level)
        {
            if (level < 0)
                level = 0;

            var research = _save.Town.Research;

            for (var i = 0; i < research.Count; i++)
            {
                var node = research[i];

                if (node.Id != id)
                    continue;

                if (node.Level == level)
                    return;

                node.Level = level;
                MarkDirty();
                return;
            }

            research.Add(new ResearchProgressData
            {
                Id = id,
                Level = level
            });
            MarkDirty();
        }

        public void SetUnitSlot(int slot, UnitKind kind)
        {
            var slots = Slots();

            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].Slot != slot)
                    continue;

                if (slots[i].Kind == kind)
                    return;

                slots[i].Kind = kind;
                MarkDirty();
                return;
            }

            slots.Add(new UnitSlotProgressData
            {
                Slot = slot,
                Kind = kind
            });
            MarkDirty();
        }

        public void ClearUnitSlot(int slot)
        {
            var slots = _save.Town.UnitSlots;

            if (slots == null)
                return;

            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].Slot != slot)
                    continue;

                slots.RemoveAt(i);
                MarkDirty();
                return;
            }
        }

        public bool IsLevelCompleted(string levelNodeId) => FindLevel(levelNodeId) != null;

        public bool TryGetLevelStars(string levelNodeId, out int stars)
        {
            var level = FindLevel(levelNodeId);
            stars = level?.Stars ?? 0;
            return level != null;
        }

        // Keeps the best result: replaying a level never lowers its stars.
        public void CompleteLevel(string levelNodeId, int stars)
        {
            if (string.IsNullOrEmpty(levelNodeId))
                return;

            if (stars < 0)
                stars = 0;

            var level = FindLevel(levelNodeId);

            if (level == null)
            {
                _save.Campaign.Levels.Add(new LevelProgressData
                {
                    Id = levelNodeId,
                    Stars = stars
                });
                MarkDirty();
                return;
            }

            if (stars <= level.Stars)
                return;

            level.Stars = stars;
            MarkDirty();
        }

        public bool TryGetSeenLevelState(string levelNodeId, out LevelNodeState state)
        {
            var seen = FindSeen(levelNodeId);
            state = seen != null ? (LevelNodeState)seen.State : LevelNodeState.Hidden;
            return seen != null;
        }

        public void SetSeenLevelState(string levelNodeId, LevelNodeState state)
        {
            if (string.IsNullOrEmpty(levelNodeId))
                return;

            var seen = FindSeen(levelNodeId);

            if (seen == null)
                _save.Campaign.Seen.Add(new LevelSeenData { Id = levelNodeId, State = (int)state });
            else if (seen.State != (int)state)
                seen.State = (int)state;
            else
                return;

            MarkDirty();
        }

        private LevelSeenData FindSeen(string levelNodeId)
        {
            if (string.IsNullOrEmpty(levelNodeId))
                return null;

            var seen = _save.Campaign.Seen;

            for (var i = 0; i < seen.Count; i++)
            {
                if (seen[i].Id == levelNodeId)
                    return seen[i];
            }

            return null;
        }

        private LevelProgressData FindLevel(string levelNodeId)
        {
            if (string.IsNullOrEmpty(levelNodeId))
                return null;

            var levels = _save.Campaign.Levels;

            for (var i = 0; i < levels.Count; i++)
            {
                if (levels[i].Id == levelNodeId)
                    return levels[i];
            }

            return null;
        }

        private List<UnitSlotProgressData> Slots()
        {
            _save.Town.UnitSlots ??= new List<UnitSlotProgressData>(UnitRoster.SlotCount);
            return _save.Town.UnitSlots;
        }

#if UNITY_EDITOR
        public void ResetResources()
        {
            SetTownResources(0, 0, 0, 0, 0);
            Flush();
        }

        public void ResetBuildings()
        {
            if (_save.Town.Buildings == null || _save.Town.Buildings.Count == 0)
                return;

            _save.Town.ClearBuildings();
            MarkDirty();
            Flush();
        }

        public void ResetResearch()
        {
            if (_save.Town.Research == null || _save.Town.Research.Count == 0)
                return;

            _save.Town.ClearResearch();
            MarkDirty();
            Flush();
        }

        public void ResetLevels()
        {
            if (_save.Campaign.Levels.Count == 0 && _save.Campaign.Seen.Count == 0)
                return;

            _save.Campaign.Levels.Clear();
            _save.Campaign.Seen.Clear();
            MarkDirty();
            Flush();
        }

        // Forget what the player saw on the map: reveal animations play again, progress stays.
        public void ResetSeenLevels()
        {
            if (_save.Campaign.Seen.Count == 0)
                return;

            _save.Campaign.Seen.Clear();
            MarkDirty();
            Flush();
        }

        public void ResetPlayer()
        {
            SetTownResources(0, 0, 0, 0, 0);

            if (_save.Campaign.Levels.Count > 0 || _save.Campaign.Seen.Count > 0)
            {
                _save.Campaign.Levels.Clear();
                _save.Campaign.Seen.Clear();
                MarkDirty();
            }

            if (_save.Town.Buildings != null && _save.Town.Buildings.Count > 0)
            {
                _save.Town.ClearBuildings();
                MarkDirty();
            }

            if (_save.Town.Research != null && _save.Town.Research.Count > 0)
            {
                _save.Town.ClearResearch();
                MarkDirty();
            }

            if (_save.Town.UnitSlots != null && _save.Town.UnitSlots.Count > 0)
            {
                _save.Town.UnitSlots.Clear();
                MarkDirty();
            }

            Flush();
        }
#endif

        public void Flush()
        {
            if (!_dirty || _save == null)
                return;

            _dirtyElapsed = 0f;

            try
            {
                _storage.Save(SaveKey, _save);
                _dirty = false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save user progress: {exception.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Flush();

            if (_hook == null)
                return;

            _hook.Unbind();
            UnityEngine.Object.Destroy(_hook.gameObject);
            _hook = null;
        }

        private void LoadOrCreate()
        {
            if (!_storage.TryLoad<UserSaveData>(SaveKey, out var save) || save == null)
            {
                _save = CreateNew();
                _dirty = true;
                Flush();
                return;
            }

            _save = save;
            _save.Town ??= new TownProgressData();
            _save.Town.Buildings ??= new List<TownBuildingProgressData>(8);
            _save.Town.Research ??= new List<ResearchProgressData>(8);
            _save.Town.UnitSlots ??= new List<UnitSlotProgressData>(UnitRoster.SlotCount);
            _save.Campaign ??= new CampaignProgressData();
            _save.Campaign.Levels ??= new List<LevelProgressData>(8);
            _save.Campaign.Seen ??= new List<LevelSeenData>(8);

            var mutated = MigrateCompletedLevels();

            if (string.IsNullOrEmpty(_save.UserId))
            {
                _save.UserId = NewUserId();
                mutated = true;
            }

            if (_save.Version != UserSaveData.CurrentVersion)
            {
                _save.Version = UserSaveData.CurrentVersion;
                mutated = true;
            }

            if (!mutated)
                return;

            _dirty = true;
            Flush();
        }

        // Version 3 kept only a list of completed node ids. They become 1-star results.
        private bool MigrateCompletedLevels()
        {
            var legacy = _save.Campaign.CompletedLevels;

            if (legacy == null || legacy.Count == 0)
            {
                _save.Campaign.CompletedLevels = null;
                return false;
            }

            for (var i = 0; i < legacy.Count; i++)
            {
                if (!string.IsNullOrEmpty(legacy[i]) && FindLevel(legacy[i]) == null)
                    _save.Campaign.Levels.Add(new LevelProgressData { Id = legacy[i], Stars = 1 });
            }

            _save.Campaign.CompletedLevels = null;
            return true;
        }

        private static UserSaveData CreateNew() => new()
        {
            Version = UserSaveData.CurrentVersion,
            UserId = NewUserId(),
            Town = new TownProgressData(),
            Campaign = new CampaignProgressData()
        };

        private static string NewUserId() => Guid.NewGuid().ToString("N");

        private void MarkDirty() => _dirty = true;

        private ProgressSaveHook CreateHook()
        {
            var hookObject = new GameObject(nameof(ProgressSaveHook));
            UnityEngine.Object.DontDestroyOnLoad(hookObject);
            var hook = hookObject.AddComponent<ProgressSaveHook>();
            hook.Bind(this);
            return hook;
        }
    }
}
