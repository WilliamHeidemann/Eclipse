using System;

namespace Game.Domain.Models.Organisms;

public static class OrganismExtensions
{
    public static Team Opposing(this Team team) => 
        team == Team.Red ? Team.Yellow : Team.Red;
    
    public static string GetText(this Card card)
    {
        return card switch
        {
            { Rank: Rank.Jack, Symbol: Symbol.Moon } => "Remove",
            { Rank: Rank.Jack, Symbol: Symbol.Sun } => "Wild",
            _ => card.Rank.AsSingleDigit()
        };
    }

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
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}