using Entitas;
using System.Collections.Generic;

namespace Code.Meta.Features.LevelMap.Systems
{
    /// <summary>
    /// Opens/closes the map on input requests. Reacts only to the input context:
    /// meta-context collectors would retain entities and break meta.Reset() of other worlds.
    /// </summary>
    public class LevelMapMenuToggleSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<MetaEntity> _menus;

        public LevelMapMenuToggleSystem() : base(Contexts.sharedInstance.input)
        {
            _menus = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.LevelMapMenu);
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(
                InputMatcher.LevelMapMenuOpenRequested.Added(),
                InputMatcher.LevelMapMenuCloseRequested.Added());

        protected override bool Filter(InputEntity entity) =>
            entity.isLevelMapMenuOpenRequested || entity.isLevelMapMenuCloseRequested;

        protected override void Execute(List<InputEntity> entities)
        {
            var menu = _menus.GetSingleEntity();

            for (var i = 0; i < entities.Count; i++)
            {
                var request = entities[i];

                if (menu != null)
                    SetOpened(menu, request.isLevelMapMenuOpenRequested && !request.isLevelMapMenuCloseRequested);

                request.Destroy();
            }
        }

        public static void SetOpened(MetaEntity menu, bool opened)
        {
            if (menu.isLevelMapMenuOpened == opened)
                return;

            menu.isLevelMapMenuOpened = opened;

            if (opened)
            {
                menu.isLevelMapDirty = true;
                menu.levelMapMenu.Value.Show();
            }
            else
            {
                menu.levelMapMenu.Value.Hide();
            }
        }
    }
}
