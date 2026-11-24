using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Code.Meta.UI.MainMenu;
using Entitas;

namespace Code.Meta.Features.LevelMap.Systems
{
    /// <summary>
    /// Keeps the level map menu entity alive. Other worlds reset the meta context on dispose,
    /// so the entity is re-created whenever it disappears.
    /// </summary>
    public class LevelMapMenuSpawnSystem : IInitializeSystem, IExecuteSystem
    {
        private readonly LevelSelectMenu _view;
        private readonly LevelCatalog _catalog;
        private readonly IGroup<MetaEntity> _menus;

        public LevelMapMenuSpawnSystem(LevelSelectMenu view, LevelCatalog catalog)
        {
            _view = view;
            _catalog = catalog;
            _menus = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.LevelMapMenu);
        }

        public void Initialize() => Execute();

        public void Execute()
        {
            if (_view == null || _menus.count > 0)
                return;

            var menu = CreateMetaEntity.Empty();
            menu.AddLevelMapMenu(_view);

            if (_catalog == null)
                return;

            menu.AddLevelMapSize(_catalog.MapSize);

            if (_catalog.MapBackground != null)
                menu.AddLevelMapBackground(_catalog.MapBackground);
        }
    }
}
