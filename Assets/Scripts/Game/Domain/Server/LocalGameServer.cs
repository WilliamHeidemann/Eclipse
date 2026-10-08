using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Domain.Models;
using UtilityToolkit.Monads;

namespace Game.Domain.Server
{
    public class LocalGameState
    {
        public LocalGameState(GameState value)
        {
            Value = value;
        }

        public GameState Value { get; set; }
    }

    public class LocalGameServer : IGameServer
    {
        private readonly LocalGameState _gameState;
        public IGameServer OtherPlayerServer { get; set; }
        public event Action<Card> OnCardReceived;
        public event Action<ClientGameState> OnOpponentPlayed;
        public event Action<int, Team> OnScored;

        public LocalGameServer(LocalGameState gameState)
        {
            _gameState = gameState;
        }

        public Task<Option<ClientGameState>> Request(Move move)
        {
            MoveValidator.MoveResult result = MoveValidator.PlayMove(_gameState.Value, move);

            _gameState.Value = result switch
            {
                MoveValidator.MoveResult.Success(var updatedState, _) => updatedState,
                MoveValidator.MoveResult.Invalid => _gameState.Value,
                _ => throw new ArgumentOutOfRangeException()
            };

            HandleEvents(result, move);

            return Task.FromResult(Option<ClientGameState>.Some(_gameState.Value.ToClientGameState(move.Team)));
        }

        private void HandleEvents(MoveValidator.MoveResult result, Move move)
        {
            if (result is MoveValidator.MoveResult.Success(var updatedState, var deltaScore))
            {
                OnCardReceived?.Invoke(updatedState.ToClientGameState(move.Team).Hand[^1]);
                
                OnScored?.Invoke(deltaScore, move.Team);

                OtherPlayerServer.Receive(updatedState.ToClientGameState(move.Team.Opposing()));
            }
        }

        public Task<ClientGameState> GetClientGameState(string matchId)
        {
            throw new NotImplementedException();
        }

        public void Receive(ClientGameState gameState)
        {
            OnOpponentPlayed?.Invoke(gameState);
        }
    }
}