using Code.Game.StaticData.Configs;

namespace Code.Game.Common.Entity.EntityBuilders
{
    public sealed partial class GameEntityBuilder
    {
        private readonly GameEntity _entity;
        private readonly EntityConfig _config;

        public GameEntityBuilder(GameEntity entity, EntityConfig config = null)
        {
            _entity = entity;
            _config = config;
        }

        public GameEntity Build()
        {
            return _entity;
        }
    }
}