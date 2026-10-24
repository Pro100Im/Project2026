using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "Game/Unit Roster")]
    public class UnitRoster : ScriptableObject
    {
        public const int SlotCount = 3;

        [SerializeField] private Sprite _lockedIcon;
        [SerializeField] private UnitRosterEntry[] _units = Array.Empty<UnitRosterEntry>();

        public Sprite LockedIcon => _lockedIcon;
        public int Count => _units?.Length ?? 0;

        public UnitRosterEntry Get(int index) => _units[index];

        public bool TryGet(UnitKind kind, out UnitRosterEntry entry)
        {
            if (_units != null)
            {
                for (var i = 0; i < _units.Length; i++)
                {
                    if (_units[i] == null || _units[i].Kind != kind)
                        continue;

                    entry = _units[i];
                    return entry.Config != null;
                }
            }

            entry = null;
            return false;
        }
    }

    [Serializable]
    public class UnitRosterEntry
    {
        [field: SerializeField] public UnitKind Kind { get; private set; }
        [field: SerializeField] public EntityConfig Config { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
    }
}
