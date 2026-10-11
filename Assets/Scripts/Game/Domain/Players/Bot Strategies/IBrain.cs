using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;

namespace Game.Domain.Players.Bot_Strategies
{
    public interface IBrain
    {
        Move DecideMove(ClientGameState clientGameState);
    }
}