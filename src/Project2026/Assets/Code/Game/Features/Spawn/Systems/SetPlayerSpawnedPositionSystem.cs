using Code.Common.Entity;
using Code.Game.Features.Network;
using Entitas;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Code.Game.Features.Player.Systems
{
    public class SetPlayerSpawnedPositionSystem : IInitializeSystem, IExecuteSystem, ITearDownSystem
    {
        private readonly IGroup<GameEntity> _players;
        private readonly IGroup<GameEntity> _spawnRequests;
        private readonly List<GameEntity> _playersBuffer = new(32);
        private readonly List<GameEntity> _spawnBuffer = new(32);

        public SetPlayerSpawnedPositionSystem(GameContext game)
        {
            _players = game.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Player,
                GameMatcher.ClientId,
                GameMatcher.ObjectId,
                GameMatcher.WaitingToSpawn,
                GameMatcher.View));

            _spawnRequests = game.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.ClientId,
                GameMatcher.ObjectId,
                GameMatcher.SpawnRequsted,
                GameMatcher.SpawnPosition));
        }

        public void Initialize()
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(RequestTypes.ReceiveSpawnPosition.ToString(), PlayerSpawnPosMessageHandler);
        }

        private void PlayerSpawnPosMessageHandler(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out Vector3 pos);
            reader.ReadValueSafe(out ulong clientID);
            reader.ReadValueSafe(out ulong objectID);

            var entity = CreateGameEntity.Empty();
            entity.AddClientId(clientID);
            entity.AddObjectId(objectID);
            entity.AddSpawnPosition(pos);
            entity.isSpawnRequsted = true;
        }

        public void Execute()
        {
            foreach (GameEntity spawnRequest in _spawnRequests.GetEntities(_spawnBuffer))
            {
                foreach (GameEntity player in _players.GetEntities(_playersBuffer))
                {
                    if (spawnRequest.clientId.Value == player.clientId.Value && spawnRequest.objectId.Value == player.objectId.Value)
                    {
                        player.transform.Value.position = spawnRequest.spawnPosition.Value;
                        player.isWaitingToSpawn = false;
                        player.isMovementAvailable = true;
                    }
                }

                spawnRequest.Destroy();
            }
        }

        public void TearDown()
        {
            NetworkManager.Singleton?.CustomMessagingManager.UnregisterNamedMessageHandler(RequestTypes.ReceiveSpawnPosition.ToString());
        }
    }
}