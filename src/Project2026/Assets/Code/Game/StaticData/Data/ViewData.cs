using Code.Infrastructure.View;
using System;
using UnityEngine;

[Serializable]
public class ViewData : EntityProperty
{
    [field: SerializeField] public EntityBehaviour Prefab;
}
