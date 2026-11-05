using System;
using System.Threading.Tasks;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using Game.Domain.Players.Bot_Strategies;
using Game.Domain.Server;
using UtilityToolkit.Monads;

namespace Game.Domain.Players
{
    public class Bot
    {
        public event Action OnStartThinking;
        
        private readonly IBrain _brain;
        private readonly LocalGameServer _gameServer;

        public Bot(LocalGameServer gameServer, IBrain brain)
        {
            _brain = brain;
            _gameServer = gameServer;
            
            gameServer.OnOpponentPlayed += async clientGameState => await Play(clientGameState);
        }

        private async Task Play(ClientGameState clientGameState)
        {
            OnStartThinking?.Invoke();
            Move move = _brain.DecideMove(clientGameState);
            Option<ClientGameState> result = await _gameServer.Request(move);
            if (result.IsSome(out _))
            {
                _gameServer.PassTurn(move.Team.Opposing());
            }
            else
            {
                throw new DomainException("Bot played an invalid move.");
            }
        }
    }
}