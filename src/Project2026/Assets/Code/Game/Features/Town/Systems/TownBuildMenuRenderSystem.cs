using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownBuildMenuRenderSystem : ReactiveSystem<TownEntity>
    {
        private readonly IGroup<TownEntity> _menus;
        private readonly IGroup<TownEntity> _buildings;

        private readonly List<TownEntity> _buildingsBuffer = new(16);

        public TownBuildMenuRenderSystem() : base(Contexts.sharedInstance.town)
        {
            var townContext = Contexts.sharedInstance.town;

            _menus = townContext.GetGroup(TownMatcher.TownBuildMenu);
            _buildings = townContext.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingType,
                    TownMatcher.TownBuildingLevel,
                    TownMatcher.TownBuildingConfig));
        }

        protected override ICollector<TownEntity> GetTrigger(IContext<TownEntity> context) =>
            context.CreateCollector(
                TownMatcher.TownBuildMenuOpened.Added(),
                TownMatcher.TownBuildingLevel.Added());

        protected override bool Filter(TownEntity entity) => entity.isEnabled;

        protected override void Execute(List<TownEntity> entities)
        {
            var menu = _menus.GetSingleEntity();

            if (menu == null || !menu.isTownBuildMenuOpened)
                return;

            var buildings = _buildings.GetEntities(_buildingsBuffer);

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                var levels = building.townBuildingConfig.Value.Levels;
                var level = building.townBuildingLevel.Value;

                for (var levelIndex = 0; levelIndex < levels.Length; levelIndex++)
                {
                    menu.townBuildMenu.Value.SetSlot(
                        building.townBuildingType.Value,
                        levelIndex,
                        levels[levelIndex],
                        GetSlotState(level, levelIndex));
                }
            }
        }

        private static TownBuildSlotState GetSlotState(int level, int levelIndex)
        {
            if (level > levelIndex)
                return TownBuildSlotState.Built;

            return level == levelIndex
                ? TownBuildSlotState.Available
                : TownBuildSlotState.Locked;
        }
    }
}
