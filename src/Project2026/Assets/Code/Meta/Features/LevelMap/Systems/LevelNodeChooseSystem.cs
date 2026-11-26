using Entitas;
using System.Collections.Generic;

namespace Code.Meta.Features.LevelMap.Systems
{
    /// <summary>
    /// Validates a node pick from the map: only non-locked nodes start a level.
    /// Writes the node id to <c>Input.SelectedLevel</c>, the game side resolves it to a LevelConfig.
    /// </summary>
    public class LevelNodeChooseSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<MetaEntity> _menus;
        private readonly IGroup<MetaEntity> _nodes;
        private readonly List<MetaEntity> _buffer = new(16);

        public LevelNodeChooseSystem() : base(Contexts.sharedInstance.input)
        {
            var meta = Contexts.sharedInstance.meta;
            _menus = meta.GetGroup(MetaMatcher.LevelMapMenu);
            _nodes = meta.GetGroup(MetaMatcher
                .AllOf(
                    MetaMatcher.LevelNode,
                    MetaMatcher.LevelNodeId,
                    MetaMatcher.LevelNodeState));
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(InputMatcher.LevelNodeChooseRequested.Added());

        protected override bool Filter(InputEntity entity) => entity.hasLevelNodeChooseRequested;

        protected override void Execute(List<InputEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var request = entities[i];
                TryChoose(request.levelNodeChooseRequested.Value);
                request.Destroy();
            }
        }

        private void TryChoose(string id)
        {
            var node = Find(id);

            if (node == null || !IsPlayable(node.levelNodeState.Value))
                return;

            Contexts.sharedInstance.input.ReplaceSelectedLevel(id);

            var menu = _menus.GetSingleEntity();

            if (menu == null)
                return;

            LevelMapMenuToggleSystem.SetOpened(menu, false);
            menu.levelMapMenu.Value.NotifyChosen(id);
        }

        private static bool IsPlayable(LevelNodeState state) =>
            state == LevelNodeState.Available || state == LevelNodeState.Completed;

        private MetaEntity Find(string id)
        {
            var nodes = _nodes.GetEntities(_buffer);

            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].levelNodeId.Value == id)
                    return nodes[i];
            }

            return null;
        }
    }
}
