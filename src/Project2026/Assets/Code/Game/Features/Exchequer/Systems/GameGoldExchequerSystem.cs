using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Exchequer.Systems
{
    public class GameGoldExchequerSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<MetaEntity> _exchequer;

        public GameGoldExchequerSystem() : base(Contexts.sharedInstance.game)
        {
            _exchequer = Contexts.sharedInstance.meta.GetGroup(MetaMatcher
                .AllOf(
                    MetaMatcher.GameExchequer,
                    MetaMatcher.ExchequerGoldCapacity));
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) =>
           context.CreateCollector(GameMatcher.ExchequerGoldChangeRequest.Added());

        protected override bool Filter(GameEntity entity) => entity.hasExchequerGoldChangeRequest;

        protected override void Execute(List<GameEntity> entities)
        {
            var exchequer = _exchequer.GetSingleEntity();

            if (exchequer == null)
                return;

            var newValue = exchequer.exchequerGoldCapacity.Value;

            for (var i = 0; i < entities.Count; i++)
                newValue += entities[i].exchequerGoldChangeRequest.Value;

            exchequer.ReplaceExchequerGoldCapacity(newValue);
            exchequer.gameExchequer.Value.SetGold(newValue);
        }
    }
}