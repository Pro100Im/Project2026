using Code.Common.Network;
using Code.Game.Features.Network;
using Entitas;
using Unity.Netcode;

namespace Code.Game.Features.Player.Systems
{
    public class SelectPlayerSpawnPositionSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _players;
        private readonly IGroup<GameEntity> _spawnPoints;

        public SelectPlayerSpawnPositionSystem(GameContext game)
        {
            _players = game.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Player,
                GameMatcher.ClientId,
                GameMatcher.ObjectId,
                GameMatcher.WaitingToSpawn,
                GameMatcher.View));

            _spawnPoints = game.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.ForPlayer,
                GameMatcher.SpawnPositions));
        }

        public void Execute()
        {
            foreach (GameEntity player in _players)
            {
                foreach (GameEntity point in _spawnPoints)
                {
                    var clientId = player.clientId.Value;
                    var objectId = player.objectId.Value;
                    var pointPos = point.spawnPositions.Value[clientId];

                    var totalSize = sizeof(float) + sizeof(float) + sizeof(float) + sizeof(ulong) + sizeof(ulong);
                    using var builder = new NetworkMessageBuilder(totalSize);
                    var writer = builder.Write(pointPos.x).Write(pointPos.y).Write(pointPos.z).Write(clientId).Write(objectId).Build();

                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                        RequestTypes.ReceiveSpawnPosition.ToString(),
                        NetworkManager.Singleton.ConnectedClientsIds,
                        writer);
                }
            }
        }
    }
}
