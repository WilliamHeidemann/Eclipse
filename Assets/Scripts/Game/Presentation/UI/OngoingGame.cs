using Game.Domain.Models.Dto;

namespace Game.Presentation.UI;

public record OngoingGame(Match Match, string OpponentId, string OpponentName, long LastMoveUnixMs)
{
    public bool IsMyTurn => Match.ClientGameState.IsMyTurn;
}
