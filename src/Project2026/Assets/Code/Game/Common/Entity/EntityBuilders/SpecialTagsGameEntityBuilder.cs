namespace Code.Game.Common.Entity.EntityBuilders
{
    public sealed partial class GameEntityBuilder
    {
        public GameEntityBuilder WithEnemyTag()
        {
            _entity.isEnemy = true;

            return this;
        }

        public GameEntityBuilder WithPlayerTag()
        {
            _entity.isPlayer = true;

            return this;
        }

        public GameEntityBuilder WithPlayerCastleTag()
        {
            _entity.isPlayerCastle = true;

            return this;
        }

        public GameEntityBuilder WithTowerTag()
        {
            _entity.isTower = true;

            return this;
        }

        public GameEntityBuilder WithTowerPlaceTag()
        {
            _entity.isTowerPlace = true;

            return this;
        }

        public GameEntityBuilder WithTargetableTag()
        {
            _entity.isTargetable = true;

            return this;
        }
    }
}