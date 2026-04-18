using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Common.Registrars
{
    public class TouchZoneRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private SpriteRenderer _touchZone;

        public override void RegisterComponents()
        {
            Entity.AddTouchZone(_touchZone);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasTouchZone)
                Entity.RemoveTouchZone();
        }
    }
}