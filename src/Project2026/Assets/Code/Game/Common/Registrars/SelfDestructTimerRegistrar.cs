using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Common.Registrars
{
    public class SelfDestructTimerRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private float _selfDestructTime;

        public override void RegisterComponents()
        {
            Entity.AddSelfDestructTimer(_selfDestructTime);
        }

        public override void UnregisterComponents()
        {
            if(Entity.hasSelfDestructTimer)
                Entity.RemoveSelfDestructTimer();
        }
    }
}