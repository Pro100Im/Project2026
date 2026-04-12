using System;
using UnityEngine;

namespace Code.Game.StaticData
{
    [Serializable]
    public abstract class EntityProperty
    {
        [field: SerializeField] public PropertyMode Mode { get; private set; }

        public void Apply(GameEntity entity)
        {
            switch (Mode)
            {
                case PropertyMode.Add:
                    Add(entity);
                    break;

                case PropertyMode.Replace:
                    Replace(entity);
                    break;

                case PropertyMode.Remove:
                    Remove(entity);
                    break;
            }
        }

        protected abstract void Add(GameEntity entity);
        protected abstract void Replace(GameEntity entity);
        protected abstract void Remove(GameEntity entity);
    }

    public enum PropertyMode
    {
        Add,
        Replace,
        Remove
    }
}