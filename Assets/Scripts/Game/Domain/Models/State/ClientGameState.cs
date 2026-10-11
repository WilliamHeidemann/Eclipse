using Game.Domain.Models.Organisms;

namespace Game.Domain.Models.State
{
    public record ClientGameState(
        Move[] Moves,
        Card[] Hand,
        Sequence[] Sequences,
        Team Team,
        bool IsMyTurn
    );
}