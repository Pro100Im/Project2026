using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerBuildSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _places;

        private readonly List<GameEntity> _buffer = new(16);

        public TowerBuildSystem()
        {
            _places = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Player,
                    GameMatcher.TowerPlace,
                    GameMatcher.TowerBuildRequest,
                    GameMatcher.EntityConfig));
        }

        public void Execute()
        {
            var places = _places.GetEntities(_buffer);

            for (var i = 0; i < places.Count; i++)
            {
                var place = places[i];

                for (var j = 0; j < place.entityConfig.Value.Properties.Length; j++)
                {
                    var property = place.entityConfig.Value.Properties[j];

                    property.Apply(place);
                }

                place.isTowerBuildRequest = false;
                place.RemoveEntityConfig();
            }
        }
    }
}