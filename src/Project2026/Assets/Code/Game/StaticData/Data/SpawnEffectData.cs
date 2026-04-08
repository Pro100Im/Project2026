using Code.Infrastructure.View;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class SpawnEffectData : EntityProperty
    {
        [field: SerializeField] public EntityBehaviour SpawnEffect { get; private set; }
    }
}