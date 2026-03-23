using Entitas;
using System.Collections.Generic;
using Unity.Netcode;

namespace Code.Game.Features.Network.Systems
{
    public class ObjectIdReceiveSystem : IInitializeSystem, ITearDownSystem
    {
        private readonly IGroup<GameEntity> _entities;
        private readonly List<GameEntity> _buffer = new(32);

        public ObjectIdReceiveSystem(GameContext game)
        {
            _entities = game.GetGroup(GameMatcher
             .AllOf(GameMatcher.ClientId)
             .NoneOf(GameMatcher.ObjectId));
        }

        public void Initialize()
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(RequestTypes.ReceiveObjectId.ToString(), ReceiveMessageHandler);
        }

        private void ReceiveMessageHandler(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong kay);
            reader.ReadValueSafe(out ulong value);

            foreach (GameEntity entity in _entities.GetEntities(_buffer))
            {
                if(entity.clientId.Value == kay)
                {
                    entity.AddObjectId(value);

                    break;
                }
            }
        }

        public void TearDown()
        {
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(RequestTypes.ReceiveObjectId.ToString());
        }
    }
}