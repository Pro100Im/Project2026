using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownBuildMenuToggleSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<TownEntity> _menus;

        public TownBuildMenuToggleSystem() : base(Contexts.sharedInstance.input)
        {
            _menus = Contexts.sharedInstance.town.GetGroup(TownMatcher.TownBuildMenu);
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
                }

                request.isTownBuildMenuToggleRequested = false;
                request.isTownBuildMenuCloseRequested = false;
                request.isDestructed = true;
            }
        }
    }
}
