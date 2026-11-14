using Code.Game.StaticData.Configs;
using Code.Infrastructure.View.Pool;
using DG.Tweening;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Town.Systems
{
    public class TownBuildEffectSystem : IExecuteSystem
    {
        private readonly IEntityViewPool _pool;
        private readonly IGroup<TownEntity> _buildings;
        private readonly List<TownEntity> _buildingsBuffer = new(16);

        public TownBuildEffectSystem(IEntityViewPool pool)
        {
            _pool = pool;
            _buildings = Contexts.sharedInstance.town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuildingBuilt,
                    TownMatcher.TownBuildingConfig));
        }

        public void Execute()
        {
            var buildings = _buildings.GetEntities(_buildingsBuffer);

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];

                if (building.hasTownBuildEffectTargets)
                {
                    var config = building.townBuildingConfig.Value;
                    var targets = building.townBuildEffectTargets.Value;

                    for (var t = 0; t < targets.Count; t++)
                    {
                        var target = targets[t];

                        if (!target)
                            continue;

                        PlayEffect(config, target.position + config.BuildEffectOffset);
                    }

                    building.RemoveTownBuildEffectTargets();
                }

                building.isTownBuildingBuilt = false;
            }
        }

        private void PlayEffect(TownBuildingConfig config, Vector3 position)
        {
            var prefab = config.BuildEffectPrefab;

            if (!prefab)
                return;

            var effect = _pool.Get(prefab, position, prefab.transform.rotation);
            var scale = config.BuildEffectScale;

            effect.transform.localScale = Vector3.one * (scale > 0f ? scale : 1f);
            effect.gameObject.SetActive(true);

            var duration = config.BuildEffectDuration > 0f ? config.BuildEffectDuration : 1.2f;

            DOVirtual.DelayedCall(duration, () => _pool.Release(effect))
                .SetLink(effect.gameObject);
        }
    }
}
