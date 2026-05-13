using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Common.Registrars
{
    public class LineRendererRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private LineRenderer _lineRenderer;

        public override void RegisterComponents()
        {
            Entity.AddLineRenderer(_lineRenderer);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasLineRenderer)
                Entity.RemoveLineRenderer();
        }
    }
}