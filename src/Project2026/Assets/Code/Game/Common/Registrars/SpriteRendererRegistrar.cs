using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Common.Registrars
{
    public class SpriteRendererRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public override void RegisterComponents()
        {
            Entity.AddSpriteRenderer(_spriteRenderer);

            _spriteRenderer.sortingOrder = Entity.sortOrder.Value;
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasSpriteRenderer)
                Entity.RemoveSpriteRenderer();
        }
    }
}