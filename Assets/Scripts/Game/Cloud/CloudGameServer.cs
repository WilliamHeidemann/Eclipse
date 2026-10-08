using System;
using System.Threading.Tasks;
using Game.Domain.Models;
using Game.Domain.Server;
using Unity.Services.CloudCode.GeneratedBindings;
using UtilityToolkit.Monads;

namespace Game.Cloud
{
    public class CloudGameServer : IGameServer
    {
        private readonly GameLogicServiceBindings _gameLogicService;
        private readonly string _matchId;
        public event Action<Card> OnCardReceived;
        public event Action<ClientGameState> OnOpponentPlayed;
        public event Action<int, Team> OnScored;

        public CloudGameServer(GameLogicServiceBindings gameLogicService, string matchId)
        {
            _gameLogicService = gameLogicService;
            _matchId = matchId;
        }

        public async Task<Option<ClientGameState>> Request(Move move)
        {
            var moveResult = await _gameLogicService.Request(move.ToDto(), _matchId);

            if (moveResult.WasValid)
            {
                OnCardReceived?.Invoke(moveResult.UpdatedGameState.ToModel().Hand[^1]);
                OnScored?.Invoke(moveResult.DeltaScore, move.Team);
                return Option<ClientGameState>.Some(moveResult.UpdatedGameState.ToModel());
            }

            return Option<ClientGameState>.None;
        }

        public async Task<ClientGameState> GetClientGameState(string matchId)
        {
            var dto = await _gameLogicService.GetClientGameState(matchId);
            return dto.ToModel();
        }

        public void Receive(ClientGameState gameState)
        {
            OnOpponentPlayed?.Invoke(gameState);
        }
    }
}