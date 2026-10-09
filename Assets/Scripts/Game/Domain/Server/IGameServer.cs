using System;
using System.Threading.Tasks;
using Game.Domain.Models;
using UtilityToolkit.Monads;

namespace Game.Domain.Server
{
    public interface IGameServer
    {
        Task<Option<ClientGameState>> Request(Move move);
        public void Receive(ClientGameState clientGameState);
        event Action<Card> OnCardReceived;
        event Action<ClientGameState> OnOpponentPlayed;
        event Action<int, Team> OnScored;
        public Task<ClientGameState> GetClientGameState(string matchId);
    }
}