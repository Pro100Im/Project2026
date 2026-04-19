using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Common.Registrars
{
    public class BoundsRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private SpriteRenderer _bounds;

        public override void RegisterComponents()
        {
            Entity.AddBounds(_bounds);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasBounds)
                Entity.RemoveBounds();
        }
    }
}