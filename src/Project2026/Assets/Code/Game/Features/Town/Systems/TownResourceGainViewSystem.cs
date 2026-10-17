using Code.Game.Features.FloatingText;
using Code.Infrastructure.View;
using Code.Infrastructure.View.Pool;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownResourceGainViewSystem : IExecuteSystem
    {
        private readonly IEntityViewPool _pool;
        private readonly EntityBehaviour _floatingTextPrefab;

        private readonly IGroup<TownEntity> _gains;
        private readonly List<TownEntity> _gainsBuffer = new(8);

        public TownResourceGainViewSystem(IEntityViewPool pool, EntityBehaviour floatingTextPrefab)
        {
            _pool = pool;
            _floatingTextPrefab = floatingTextPrefab;

            _gains = Contexts.sharedInstance.town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownResourceGained,
                    TownMatcher.TownView));
        }

        public void Execute()
        {
            var gains = _gains.GetEntities(_gainsBuffer);

            for (var i = 0; i < gains.Count; i++)
            {
                var building = gains[i];

                if (_floatingTextPrefab)
                    ShowGain(building);

                building.RemoveTownResourceGained();
            }
        }

        private void ShowGain(TownEntity building)
        {
            var view = _pool.Get(
                _floatingTextPrefab,
                building.townView.Value.transform.position,
                _floatingTextPrefab.transform.rotation);

            view.gameObject.SetActive(true);

            var text = view.GetComponent<FloatingTextView>();

            if (!text)
            {
                _pool.Release(view);
                return;
            }

            text.Play($"+{building.townResourceGained.Value}", () => _pool.Release(view));
        }
    }
}
