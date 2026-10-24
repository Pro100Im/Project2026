using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownBuildMenuToggleSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<TownEntity> _menus;
        private readonly IGroup<TownEntity> _researchMenus;
        private readonly IGroup<TownEntity> _equipmentMenus;

        public TownBuildMenuToggleSystem() : base(Contexts.sharedInstance.input)
        {
            var town = Contexts.sharedInstance.town;
            _menus = town.GetGroup(TownMatcher.TownBuildMenu);
            _researchMenus = town.GetGroup(TownMatcher.TownResearchMenu);
            _equipmentMenus = town.GetGroup(TownMatcher.TownEquipmentMenu);
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(
                InputMatcher.TownBuildMenuToggleRequested.Added(),
                InputMatcher.TownBuildMenuCloseRequested.Added());

        protected override bool Filter(InputEntity entity) =>
            entity.isTownBuildMenuToggleRequested || entity.isTownBuildMenuCloseRequested;

        protected override void Execute(List<InputEntity> entities)
        {
            var menu = _menus.GetSingleEntity();

            for (var i = 0; i < entities.Count; i++)
            {
                var request = entities[i];

                if (menu != null)
                {
                    if (request.isTownBuildMenuCloseRequested)
                        menu.isTownBuildMenuOpened = false;
                    else if (request.isTownBuildMenuToggleRequested)
                        menu.isTownBuildMenuOpened = !menu.isTownBuildMenuOpened;

                    if (menu.isTownBuildMenuOpened)
                    {
                        CloseResearchMenu();
                        CloseEquipmentMenu();
                    }
                }

                request.isTownBuildMenuToggleRequested = false;
                request.isTownBuildMenuCloseRequested = false;
                request.isDestructed = true;
            }
        }

        private void CloseResearchMenu()
        {
            var researchMenu = _researchMenus.GetSingleEntity();

            if (researchMenu != null)
                researchMenu.isTownResearchMenuOpened = false;
        }

        private void CloseEquipmentMenu()
        {
            var equipmentMenu = _equipmentMenus.GetSingleEntity();

            if (equipmentMenu != null)
                equipmentMenu.isTownEquipmentMenuOpened = false;
        }
    }
}
