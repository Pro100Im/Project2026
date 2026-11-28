using Code.Game.Features.FloatingText.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.FloatingText
{
    /// <summary>Floating numbers: age them, then draw all of them as one batched mesh.</summary>
    public class FloatingTextFeature : Feature
    {
        public FloatingTextFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<FloatingTextTickSystem>());
            Add(systemFactory.Create<FloatingTextRenderSystem>());
        }
    }
}
