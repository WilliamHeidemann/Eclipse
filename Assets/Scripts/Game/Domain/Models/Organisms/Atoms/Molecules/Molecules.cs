using System;

namespace Game.Domain.Models.Organisms.Atoms.Molecules;

public enum Column
{
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight
}

public enum Row
{
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
}

public enum Rank
{
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King
}

public enum Symbol
{
    Sun,
    Moon
}

public enum Team
{
    Red,
    Yellow
}

public static class MoleculeExtensions
{
    public static string AsSingleDigit(this Rank rank)
    {
        return rank switch
        {
            Rank.Ace => "A",
            Rank.Two => "2",
            Rank.Three => "3",
            Rank.Four => "4",
            Rank.Five => "5",
            Rank.Six => "6",
            Rank.Seven => "7",
            Rank.Eight => "8",
            Rank.Nine => "9",
            Rank.Ten => "10",
            Rank.Jack => string.Empty,
            Rank.Queen => "Q",
            Rank.King => "K",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public static Team Opposing(this Team team) => 
        team == Team.Red ? Team.Yellow : Team.Red;
}