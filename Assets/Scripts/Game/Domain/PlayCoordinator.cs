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
    public class PlayCoordinator
    {
        private readonly IGameServer _gameServer;
        private ClientGameState _clientGameState;

        public event Action<Position> OnInvalidMoveRequest;
        public event Action<Move> OnValidMoveRequest;
        public event Action<Card> OnDrawCard;
        public event Action<Move> OnOpponentPlayed;
        public event Action<Sequence> OnSequenceScored;
        public event Action<Team> OnTurnPassedTo;
        public event Action<Team> OnTeamWon;

        public PlayCoordinator(IGameServer gameServer, ClientGameState startingState)
        {
            _gameServer = gameServer;
            _clientGameState = startingState;

            gameServer.OnCardReceived += card => OnDrawCard?.Invoke(card);
            gameServer.OnOpponentPlayed += Receive;
        }

        public async Task PositionClicked(Position position)
        {
            Option<Move> attempt = MoveValidator.GetValidMove(_clientGameState, position);

            if (attempt.IsSome(out Move move))
            {
                OnValidMoveRequest?.Invoke(move);
                OnTurnPassedTo?.Invoke(move.Team.Opposing());
                Option<ClientGameState> result = await _gameServer.Request(move);
                if (result.IsSome(out ClientGameState clientGameState))
                {
                    _clientGameState = clientGameState;
                    CheckForSequenceInLastMove(clientGameState, clientGameState.Team);
                }
            }
            else
            {
                OnInvalidMoveRequest?.Invoke(position);
            }
        }

        public void RaiseStartGameEvents()
        {
            foreach (Card card in _clientGameState.Hand)
            {
                OnDrawCard?.Invoke(card);
            }
            
            OnTurnPassedTo?.Invoke(_clientGameState.Team);
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
            
            CheckForSequenceInLastMove(clientGameState, clientGameState.Team.Opposing());
            
            OnTurnPassedTo?.Invoke(clientGameState.Team);
        }

        private void CheckForSequenceInLastMove(ClientGameState clientGameState, Team ofTeam)
        {
            foreach (var sequence in clientGameState.SequencesFromLastMove(ofTeam))
            {
                OnSequenceScored?.Invoke(sequence);
            }
            
            if (clientGameState.Sequences.Count(s => s.Team == ofTeam) >= 2)
            {
                OnTeamWon?.Invoke(ofTeam);
            }
        }
    }
}