using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class HealthData : EntityProperty
    {
        [field: SerializeField] public float Health { get; private set; }
        [field: SerializeField] public float Regen { get; private set; }
    }
}