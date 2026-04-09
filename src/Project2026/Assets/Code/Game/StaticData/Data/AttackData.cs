using Code.Infrastructure.View;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class AttackData : EntityProperty
    {
        [field: SerializeField] public EntityBehaviour AttackHitEffect { get; private set; }
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float Range { get; private set; }
        [field: SerializeField] public float Cooldown { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }
        [field: SerializeField] public bool  IsMelee { get; private set; }
    }
}