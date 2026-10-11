using Game.Domain.Models;
using Game.Domain.Models.Organisms.Atoms;
using Game.Domain.Models.State;
using Game.Domain.Players.Bot_Strategies;
using Game.Domain.Server;

namespace Game.Domain.Players
{
    public class Bot
    {
        private readonly IBrain _brain;
        private readonly IGameServer _gameServer;

        public Bot(IGameServer gameServer, IBrain brain)
        {
            _brain = brain;
            _gameServer = gameServer;
            
            gameServer.OnOpponentPlayed += Play;
        }

        private void Play(ClientGameState clientGameState)
        {
            Move move = _brain.DecideMove(clientGameState);
            _gameServer.Request(move);
        }
    }
}