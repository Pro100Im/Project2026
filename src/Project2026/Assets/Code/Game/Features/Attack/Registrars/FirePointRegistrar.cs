using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.Attack.Registrars
{
    public class FirePointRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Transform _firePoint;

        public override void RegisterComponents()
        {
            if(!Entity.hasFirePoint)
                Entity.AddFirePoint(_firePoint.position);
        }

        public override void UnregisterComponents()
        {
            if(Entity.hasFirePoint)
                Entity.RemoveFirePoint();
        }

        private void OnDrawGizmosSelected()
        {
            if(_firePoint == null) 
                return;

            Gizmos.color = Color.red;

            Gizmos.DrawSphere(_firePoint.position, 0.1f);
        }
    }
}