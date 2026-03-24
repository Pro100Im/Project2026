using Code.Common.Entity;
using Code.Infrastructure.Identifiers;
using UnityEngine;

namespace Code.Game.Features.Player.Factory
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly IIdentifierService _identifiers;

        public PlayerFactory(IIdentifierService identifiers)
        {
            _identifiers = identifiers;
        }

        public GameEntity CreatePlayer(ulong id, int playerAreaNumber)
        {
            var entity = CreateGameEntity.Empty();
            entity.AddId(_identifiers.Next());
            entity.AddPlayerAreaNumber(playerAreaNumber);
            entity.AddClientId(id);
            entity.AddDirection(Vector2.zero);
            entity.AddLookAtPoint(Vector2.zero);
            entity.AddSpeed(1f);
            entity.AddMaxRunSpeed(4f);
            entity.AddMaxWalkSpeed(2f);
            entity.AddCurrentSpeed(0f);
            entity.AddViewPath("Game/Player/Player");
            entity.isWaitingToSpawn = true;
            entity.isRotationAlignedAlongTarget = true;
            entity.isPlayer = true;

            return entity;
        }
    }
}
