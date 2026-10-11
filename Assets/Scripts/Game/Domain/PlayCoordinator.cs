using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using Game.Domain.Server;
using UtilityToolkit.CollectionExtensions;
using UtilityToolkit.Monads;

namespace Game.Domain
{
    public readonly record struct Play(Move Move, Card Card);

    public class PlayCoordinator
    {
        private readonly IGameServer _gameServer;
        private ClientGameState _clientGameState;

        public event Action<Position> OnInvalidMoveRequest;
        public event Action<Play> OnValidMoveRequest;
        public event Action<Card> OnDrawCard;
        public event Action<Move> OnOpponentPlayed;
        public event Action<Sequence> OnSequenceScored;

        public PlayCoordinator(IGameServer gameServer, ClientGameState startingState)
        {
            _gameServer = gameServer;
            _clientGameState = startingState;

            gameServer.OnCardReceived += card => OnDrawCard?.Invoke(card);
            gameServer.OnOpponentPlayed += Receive;
        }

        public async Task PositionClicked(Position position)
        {
            Option<Play> attempt = MoveValidator.GetValidPlay(_clientGameState, position);

            if (attempt.IsSome(out Play play))
            {
                OnValidMoveRequest?.Invoke(play);
                Option<ClientGameState> result = await _gameServer.Request(play.Move);
                if (result.IsSome(out ClientGameState clientGameState))
                {
                    _clientGameState = clientGameState;
                }
            }
            else
            {
                OnInvalidMoveRequest?.Invoke(position);
            }
        }

        public void RaiseDrawHandEvent()
        {
            foreach (Card card in _clientGameState.Hand)
            {
                OnDrawCard?.Invoke(card);
            }
        }

        public async Task CheckIfOpponentPlayed(string matchId)
        {
            ClientGameState clientGameState = await _gameServer.GetClientGameState(matchId);
            if (_clientGameState.Moves.Length < clientGameState.Moves.Length)
            {
                _gameServer.Receive(clientGameState);
            }
        }

        private void Receive(ClientGameState clientGameState)
        {
            _clientGameState = clientGameState;

            clientGameState.Moves.LastOption().Try(lastMove => OnOpponentPlayed?.Invoke(lastMove));
        }
    }
}