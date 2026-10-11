using Game.Domain.Models.Organisms;
using Game.Domain.Models.Organisms.Atoms;
using Game.Domain.Models.Organisms.Atoms.Molecules;

namespace Game.Domain.Models.State
{
    public record ClientGameState(
        Move[] Moves,
        Card[] Hand,
        Position[] Locked,
        Team Team,
        Score Score,
        bool IsMyTurn
    );
}