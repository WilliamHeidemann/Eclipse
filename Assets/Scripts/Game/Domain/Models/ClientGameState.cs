using System;

namespace Game.Domain.Models
{
    public class ClientGameState
    {
        // Last move: last entry of Moves
        public Move[] Moves { get; set; } = Array.Empty<Move>();
        public Card[] Hand { get; set; } = Array.Empty<Card>();
        public Position[] Locked { get; set; } = Array.Empty<Position>();
        public Team Team { get; set; }
        public Score Score { get; set; } = new();
        public bool IsMyTurn { get; set; }
    }
}