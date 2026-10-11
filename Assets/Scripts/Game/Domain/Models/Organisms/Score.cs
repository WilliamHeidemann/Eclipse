using System;
using Game.Domain.Models.Organisms.Atoms.Molecules;

namespace Game.Domain.Models.Organisms
{
    public record Score(int Red, int Yellow)
    {
        public int Get(Team team) =>
            team switch
            {
                Team.Red => Red,
                Team.Yellow => Yellow,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            };
    }
}