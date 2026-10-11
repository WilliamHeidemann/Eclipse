using System;
using System.Threading.Tasks;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using UtilityToolkit.Monads;

namespace Game.Domain.Server
{
    public interface IGameServer
    {
        Task<Option<ClientGameState>> Request(Move move);
        public void Receive(ClientGameState clientGameState);
        event Action<Card> OnCardReceived;
        event Action<ClientGameState> OnOpponentPlayed;
        public Task<ClientGameState> GetClientGameState(string matchId);
    }
}