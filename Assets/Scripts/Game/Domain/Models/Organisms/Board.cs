using System.Collections.Generic;
using System.Linq;

namespace Game.Domain.Models.Organisms
{
    public class Board
    {
        private readonly Dictionary<Position, Team> _takenSpaces = new();

        public bool TryAdd(Position position, Team team) => _takenSpaces.TryAdd(position, team);

        public bool Fits(Position position) => !_takenSpaces.ContainsKey(position);

        // public bool HasSequence(Team team) =>
        //     SequencePatterns.All().Any(pattern => IsSequence(pattern, team));
        //
        // public int SequenceCount(Team team) =>
        //     SequencePatterns.All().Count(pattern => IsSequence(pattern, team));

        private bool IsSequence(Position[] positions, Team team) =>
            positions.All(position =>
                _takenSpaces.TryGetValue(position, out Team occupyingTeam) && occupyingTeam == team);
        
        public Board(Move[] moves)
        {
            foreach (Move move in moves)
            {
                if (!_takenSpaces.TryAdd(move.Position, move.Team))
                {
                    throw new DomainException($"Duplicate move at {BoardLayout.Get(move.Position)}");
                }
            }
        }

        public bool Remove(Position position)
        {
            return _takenSpaces.Remove(position);
        }

        public bool OwnerIs(Position position, Team team) => 
            _takenSpaces.TryGetValue(position, out Team owner) && owner == team;
    }
}