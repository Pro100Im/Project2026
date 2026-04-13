using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerBuildSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _places;

        private readonly List<GameEntity> _buffer = new(32);

        public TowerBuildSystem(GameContext gameContext)
        {
            _places = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Player,
                GameMatcher.TowerPlace,
                GameMatcher.TowerBuildRequest,
                GameMatcher.EntityConfig));
        } 

        public void Execute()
        {
            foreach (var place in _places.GetEntities(_buffer))
            {
                foreach (var property in place.entityConfig.Value.Properties)
                {
                    property.Apply(place);
                }

                place.isTowerBuildRequest = false;
                place.RemoveEntityConfig();
            }
        }
    }
}