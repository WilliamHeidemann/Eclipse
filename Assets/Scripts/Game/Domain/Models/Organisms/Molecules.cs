using System;

namespace Game.Domain.Models.Organisms;

public enum Column : byte
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

public enum Row : byte
{
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
}

public enum Rank : byte
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

public enum Symbol : byte
{
    Sun,
    Moon
}

public enum Team : byte
{
    Red,
    Yellow
}