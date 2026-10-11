using System.Collections.Generic;
using System.Linq;

namespace Game.Domain.Models.Organisms
{
    public class MoveHistory
    {
        private List<Move> Moves { get; }

        public MoveHistory(Move[] moves)
        {
            Moves = moves.ToList();
        }

        public void Add(Move move)
        {
            Moves.Add(move);
        }

        public void Clear()
        {
            Moves.Clear();
        }

        public Move[] GetMoves()
        {
            return Moves.ToArray();
        }
    }
}