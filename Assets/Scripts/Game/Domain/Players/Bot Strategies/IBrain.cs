using Game.Domain.Models;
using Game.Domain.Models.Organisms.Atoms;

namespace Game.Domain.Players.Bot_Strategies
{
    public interface IBrain
    {
        Move DecideMove(ClientGameState clientGameState);
    }
}