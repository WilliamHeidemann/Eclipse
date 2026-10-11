using System;

namespace Game.Domain.Models
{
    [Serializable]
    public readonly struct Move
    {
        public Position Position { get; }
        public Card Card { get; }
        public Team Team { get; }

        public Move(Position position, Card card, Team team)
        {
            Position = position;
            Card = card;
            Team = team;
        }
    }
}