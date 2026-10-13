 using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models.Organisms;
using UtilityToolkit.CollectionExtensions;

namespace Game.Domain.Models.State;

public static class StateExtensions
{
    public static IEnumerable<Sequence> SequencesFromLastMove(this ClientGameState clientGameState, Team ofTeam)
    {
        if (clientGameState.Moves.LastOption(m => m.Team == ofTeam).IsSome(out Move lastMove))
        {
            return clientGameState.Sequences.Where(s => s.Positions.Contains(lastMove.Position));
        }

        return Enumerable.Empty<Sequence>();
    }
}