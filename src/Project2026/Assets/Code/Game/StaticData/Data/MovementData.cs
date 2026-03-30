using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class MovementData : EntityProperty
    {
        [field: SerializeField] public float Speed { get; private set; }
    }
}