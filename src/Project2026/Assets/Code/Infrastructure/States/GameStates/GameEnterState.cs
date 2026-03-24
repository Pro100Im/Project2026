using Code.Common.Entity;
using Code.Common.Network;
using Code.Common.UI.Transition;
using Code.Game.Features.Network;
using Code.Game.Features.Player.Factory;
using Code.Infrastructure.States.StateInfrastructure;
using Code.Infrastructure.States.StateMachine;
using Cysharp.Threading.Tasks;
using Entitas;
using Unity.Netcode;
using UnityEngine;

namespace Code.Infrastructure.States.GameStates
{
    public class GameEnterState : SimpleState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly IPlayerFactory _playerFactory;
        private readonly INetworkSessionService _networkSessionService;
        private readonly IGroup<GameEntity> _entities;

        private readonly TransitionScreen _transitionScreen;

        public GameEnterState(IGameStateMachine stateMachine, IPlayerFactory playerFactory, INetworkSessionService networkSessionService, TransitionScreen transitionScreen, 
            GameContext game)
        {
            _stateMachine = stateMachine;
            _playerFactory = playerFactory;
            _networkSessionService = networkSessionService;
            _transitionScreen = transitionScreen;

            _entities = game.GetGroup(GameMatcher.AllOf(GameMatcher.ClientId));
        }

        public override void Enter()
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(RequestTypes.CreatePlayerEntity.ToString(), CreatePlayerMessageHandler);
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += SceneManager_OnSynchronizeComplete;
        }

        private void SceneManager_OnSynchronizeComplete(ulong clientId)
        {
            var totalClients = NetworkManager.Singleton.ConnectedClientsList.Count;

            if (totalClients < _networkSessionService.GetMaxPlayersCount()) 
            {
                Debug.Log($"totalClients {totalClients}");

                return;
            }

            if (NetworkManager.Singleton.IsHost)
            {
                for (int i = 0; i < totalClients; i++)
                {
                    var id = NetworkManager.Singleton.ConnectedClientsList[i].ClientId;
                    var totalSize = sizeof(ulong);
                    using var builder = new NetworkMessageBuilder(totalSize);
                    var writer = builder.Write(id).Build();

                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                        RequestTypes.CreatePlayerEntity.ToString(),
                        NetworkManager.Singleton.ConnectedClientsIds,
                        writer);
                }
            }
        }

        private void CreatePlayerMessageHandler(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong playerId);

            _playerFactory.CreatePlayer(playerId, _entities.count);

            var entity = CreateInputEntity.Empty();

            entity.isInput = true;
            entity.AddClientId(playerId);
            entity.AddAxisInput(Vector2.zero);
            entity.AddPointerInput(Vector2.zero);
            entity.AddPointerRay(new Ray());

            if (NetworkManager.Singleton.LocalClientId == playerId)
            {
                entity.isLocalPlayer = true;
            }

            if(_entities.count >= NetworkManager.Singleton.ConnectedClientsList.Count)
            {
                _stateMachine.Enter<GameLoopState>();
                _transitionScreen.Hide().AsTask();
            }
        }

        protected override void Exit()
        {
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= SceneManager_OnSynchronizeComplete;
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(RequestTypes.CreatePlayerEntity.ToString());
        }
    }
}