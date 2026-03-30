using Code.Infrastructure.View;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class ViewData : EntityProperty
    {
        [field: SerializeField] public EntityBehaviour Prefab { get; private set; }
    }
}