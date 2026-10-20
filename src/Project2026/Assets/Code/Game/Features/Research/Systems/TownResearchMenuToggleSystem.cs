using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class TownResearchMenuToggleSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<TownEntity> _menus;
        private readonly IGroup<TownEntity> _buildMenus;

        public TownResearchMenuToggleSystem() : base(Contexts.sharedInstance.input)
        {
            var town = Contexts.sharedInstance.town;
            _menus = town.GetGroup(TownMatcher.TownResearchMenu);
            _buildMenus = town.GetGroup(TownMatcher.TownBuildMenu);
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(
                InputMatcher.TownResearchMenuToggleRequested.Added(),
                InputMatcher.TownResearchMenuCloseRequested.Added());

        protected override bool Filter(InputEntity entity) =>
            entity.isTownResearchMenuToggleRequested || entity.isTownResearchMenuCloseRequested;

        protected override void Execute(List<InputEntity> entities)
        {
            var menu = _menus.GetSingleEntity();

            for (var i = 0; i < entities.Count; i++)
            {
                var request = entities[i];

                if (menu != null)
                {
                    if (request.isTownResearchMenuCloseRequested)
                        menu.isTownResearchMenuOpened = false;
                    else if (request.isTownResearchMenuToggleRequested)
                        menu.isTownResearchMenuOpened = !menu.isTownResearchMenuOpened;

                    if (menu.isTownResearchMenuOpened)
                        CloseBuildMenu();
                }

                request.isTownResearchMenuToggleRequested = false;
                request.isTownResearchMenuCloseRequested = false;
                request.isDestructed = true;
            }
        }

        private void CloseBuildMenu()
        {
            var buildMenu = _buildMenus.GetSingleEntity();

            if (buildMenu != null)
                buildMenu.isTownBuildMenuOpened = false;
        }
    }
}
