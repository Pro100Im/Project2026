using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "Entity/EntityConfig")]
    public class EntityConfig : ScriptableObject
    {
        [SerializeReference]
        public EntityProperty[] Properties;

        private Dictionary<Type, EntityProperty> _propertyMap;

        private void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            _propertyMap = Properties?.ToDictionary(p => p.GetType(), p => p)
                          ?? new Dictionary<Type, EntityProperty>();
        }

        public T GetProperty<T>() where T : EntityProperty
        {
            _propertyMap.TryGetValue(typeof(T), out var prop);

            return prop as T;
        }
    }
}