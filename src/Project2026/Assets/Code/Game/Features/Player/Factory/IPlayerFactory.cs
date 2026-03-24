using UnityEngine;

namespace Code.Game.Features.Player.Factory
{
    public interface IPlayerFactory
    {
        GameEntity CreatePlayer(ulong clientId, int playerAreaNumber);
    }
}