using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownResourceStockSystem : ReactiveSystem<TownEntity>
    {
        private readonly IGroup<TownEntity> _exchequer;

        public TownResourceStockSystem() : base(Contexts.sharedInstance.town)
        {
            _exchequer = Contexts.sharedInstance.town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownExchequer,
                    TownMatcher.TownResources));
        }

        protected override ICollector<TownEntity> GetTrigger(IContext<TownEntity> context) =>
            context.CreateCollector(TownMatcher.TownResourceGained.Added());

        protected override bool Filter(TownEntity entity) =>
            entity.hasTownResourceGained && entity.hasTownBuildingConfig;

        protected override void Execute(List<TownEntity> entities)
        {
            var exchequer = _exchequer.GetSingleEntity();

            if (exchequer == null)
                return;

            var gold = exchequer.townGold.Value;
            var stone = exchequer.townStone.Value;
            var crystal = exchequer.townCrystal.Value;
            var wood = exchequer.townWood.Value;
            var knowledge = exchequer.townKnowledge.Value;

            var goldChanged = false;
            var stoneChanged = false;
            var crystalChanged = false;
            var woodChanged = false;
            var knowledgeChanged = false;

            for (var i = 0; i < entities.Count; i++)
            {
                var building = entities[i];
                var amount = building.townResourceGained.Value;

                switch (building.townBuildingConfig.Value.Resource)
                {
                    case TownResource.Gold:
                        gold += amount;
                        goldChanged = true;
                        break;
                    case TownResource.Stone:
                        stone += amount;
                        stoneChanged = true;
                        break;
                    case TownResource.Crystal:
                        crystal += amount;
                        crystalChanged = true;
                        break;
                    case TownResource.Wood:
                        wood += amount;
                        woodChanged = true;
                        break;
                    case TownResource.Knowledge:
                        knowledge += amount;
                        knowledgeChanged = true;
                        break;
                }
            }

            var view = exchequer.townExchequer.Value;

            if (goldChanged)
            {
                exchequer.ReplaceTownGold(gold);
                view.SetGold(gold);
            }

            if (stoneChanged)
            {
                exchequer.ReplaceTownStone(stone);
                view.SetStone(stone);
            }

            if (crystalChanged)
            {
                exchequer.ReplaceTownCrystal(crystal);
                view.SetCrystal(crystal);
            }

            if (woodChanged)
            {
                exchequer.ReplaceTownWood(wood);
                view.SetWood(wood);
            }

            if (knowledgeChanged)
            {
                exchequer.ReplaceTownKnowledge(knowledge);
                view.SetKnowledge(knowledge);
            }
        }
    }
}
