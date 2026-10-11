using Game.Domain.Models.Organisms;

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