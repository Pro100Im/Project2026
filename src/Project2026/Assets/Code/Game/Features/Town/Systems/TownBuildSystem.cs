using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownBuildSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<TownEntity> _menus;
        private readonly IGroup<TownEntity> _buildings;

        private readonly List<TownEntity> _buildingsBuffer = new(16);

        public TownBuildSystem() : base(Contexts.sharedInstance.input)
        {
            var townContext = Contexts.sharedInstance.town;

            _menus = townContext.GetGroup(TownMatcher.TownBuildMenu);
            _buildings = townContext.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingType,
                    TownMatcher.TownBuildingLevel,
                    TownMatcher.TownBuildingMaxLevel));
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(InputMatcher.TownBuildRequested.Added());

        protected override bool Filter(InputEntity entity) => entity.hasTownBuildRequested;

        protected override void Execute(List<InputEntity> entities)
        {
            var menu = _menus.GetSingleEntity();
            var isMenuOpened = menu != null && menu.isTownBuildMenuOpened;

            for (var i = 0; i < entities.Count; i++)
            {
                var request = entities[i];

                if (isMenuOpened)
                {
                    menu.isTownBuildMenuOpened = false;
                    Build(request.townBuildRequested.Value);
                }

                request.RemoveTownBuildRequested();
                request.isDestructed = true;
            }
        }

        private void Build(BuildingType type)
        {
            var buildings = _buildings.GetEntities(_buildingsBuffer);

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];

                if (building.townBuildingType.Value != type)
                    continue;

                var level = building.townBuildingLevel.Value;

                if (level >= building.townBuildingMaxLevel.Value)
                    continue;

                building.ReplaceTownBuildingLevel(level + 1);
            }
        }
    }
}
