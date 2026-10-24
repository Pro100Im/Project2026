using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownEquipmentMenuToggleSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<TownEntity> _menus;
        private readonly IGroup<TownEntity> _buildMenus;
        private readonly IGroup<TownEntity> _researchMenus;

        public TownEquipmentMenuToggleSystem() : base(Contexts.sharedInstance.input)
        {
            var town = Contexts.sharedInstance.town;
            _menus = town.GetGroup(TownMatcher.TownEquipmentMenu);
            _buildMenus = town.GetGroup(TownMatcher.TownBuildMenu);
            _researchMenus = town.GetGroup(TownMatcher.TownResearchMenu);
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(
                InputMatcher.TownEquipmentMenuToggleRequested.Added(),
                InputMatcher.TownEquipmentMenuCloseRequested.Added());

        protected override bool Filter(InputEntity entity) =>
            entity.isTownEquipmentMenuToggleRequested || entity.isTownEquipmentMenuCloseRequested;

        protected override void Execute(List<InputEntity> entities)
        {
            var menu = _menus.GetSingleEntity();

            for (var i = 0; i < entities.Count; i++)
            {
                var request = entities[i];

                if (menu != null)
                {
                    if (request.isTownEquipmentMenuCloseRequested)
                        menu.isTownEquipmentMenuOpened = false;
                    else if (request.isTownEquipmentMenuToggleRequested)
                        menu.isTownEquipmentMenuOpened = !menu.isTownEquipmentMenuOpened;

                    if (menu.isTownEquipmentMenuOpened)
                        CloseOthers();
                }

                request.isTownEquipmentMenuToggleRequested = false;
                request.isTownEquipmentMenuCloseRequested = false;
                request.isDestructed = true;
            }
        }

        private void CloseOthers()
        {
            var buildMenu = _buildMenus.GetSingleEntity();

            if (buildMenu != null)
                buildMenu.isTownBuildMenuOpened = false;

            var researchMenu = _researchMenus.GetSingleEntity();

            if (researchMenu != null)
                researchMenu.isTownResearchMenuOpened = false;
        }
    }
}
