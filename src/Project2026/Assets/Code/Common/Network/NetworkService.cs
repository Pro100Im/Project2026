using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;

namespace Code.Common.Network
{
    public class NetworkService : INetworkConnectionService, INetworkSessionService
    {
        private ISession _session;

        private CancellationTokenSource _cancellationTokenSource;

        public void QuickMatch()
        {
            if(_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            JoinOrCreateMatchmakerGameAsync(_cancellationTokenSource.Token);
        }

        public int GetMaxPlayersCount()
        {
            return _session.MaxPlayers;
        }

        public void CancelSerching()
        {
            _cancellationTokenSource?.Cancel();
        }

        public async void JoinOrCreateMatchmakerGameAsync(CancellationToken cancellationToken)
        {
            await StartServicesAsync();

            var sessionOptions = new SessionOptions()
            {
                MaxPlayers = 2
            }.WithRelayNetwork();

            var matchOptions = new MatchmakerOptions
            {
                QueueName = "Test",
            };

            _session = await MultiplayerService.Instance.MatchmakeSessionAsync(matchOptions, sessionOptions, cancellationToken);
        }

        private async Task StartServicesAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsAuthorized)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}