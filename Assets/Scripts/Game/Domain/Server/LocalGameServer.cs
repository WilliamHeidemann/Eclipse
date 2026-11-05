using System;
using System.Threading.Tasks;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
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
        public event Action<ClientGameState> OnOpponentPlayed;

        public LocalGameServer(LocalGameState gameState)
        {
            _gameState = gameState;
        }

        public Task<Option<ClientGameState>> Request(Move move)
        {
            MoveResult result = MoveValidator.PlayMove(_gameState.Value, move);

            _gameState.Value = result switch
            {
                MoveResult.Success success => success.UpdatedState,
                MoveResult.Invalid => _gameState.Value,
                _ => throw new ArgumentOutOfRangeException()
            };

            return Task.FromResult(Option<ClientGameState>.Some(_gameState.Value.ToClientGameState(move.Team)));
        }

        public void PassTurn(Team team)
        {
            OtherPlayerServer.Receive(_gameState.Value.ToClientGameState(team));
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