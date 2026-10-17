using Code.Game.Features.Town;
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

        public void ResetPlayer()
        {
            SetTownResources(0, 0, 0, 0, 0);

            if (_save.Town.Buildings != null && _save.Town.Buildings.Count > 0)
            {
                _save.Town.ClearBuildings();
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

            var mutated = false;

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

        private static UserSaveData CreateNew() => new()
        {
            Version = UserSaveData.CurrentVersion,
            UserId = NewUserId(),
            Town = new TownProgressData()
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
