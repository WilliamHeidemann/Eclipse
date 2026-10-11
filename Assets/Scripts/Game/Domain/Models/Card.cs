using System;

namespace Game.Domain.Models
{
    [Serializable]
    public readonly struct Card : IEquatable<Card>
    {
        public static Card TwoOfMoon => new(Symbol.Moon, Rank.Two);
        public static Card ThreeOfMoon => new(Symbol.Moon, Rank.Three);
        public static Card FourOfMoon => new(Symbol.Moon, Rank.Four);
        public static Card FiveOfMoon => new(Symbol.Moon, Rank.Five);
        public static Card SixOfMoon => new(Symbol.Moon, Rank.Six);
        public static Card SevenOfMoon => new(Symbol.Moon, Rank.Seven);
        public static Card EightOfMoon => new(Symbol.Moon, Rank.Eight);
        public static Card NineOfMoon => new(Symbol.Moon, Rank.Nine);
        public static Card TenOfMoon => new(Symbol.Moon, Rank.Ten);
        public static Card JackOfMoon => new(Symbol.Moon, Rank.Jack);
        public static Card QueenOfMoon => new(Symbol.Moon, Rank.Queen);
        public static Card KingOfMoon => new(Symbol.Moon, Rank.King);
        public static Card AceOfMoon => new(Symbol.Moon, Rank.Ace);

        public static Card TwoOfSun => new(Symbol.Sun, Rank.Two);
        public static Card ThreeOfSun => new(Symbol.Sun, Rank.Three);
        public static Card FourOfSun => new(Symbol.Sun, Rank.Four);
        public static Card FiveOfSun => new(Symbol.Sun, Rank.Five);
        public static Card SixOfSun => new(Symbol.Sun, Rank.Six);
        public static Card SevenOfSun => new(Symbol.Sun, Rank.Seven);
        public static Card EightOfSun => new(Symbol.Sun, Rank.Eight);
        public static Card NineOfSun => new(Symbol.Sun, Rank.Nine);
        public static Card TenOfSun => new(Symbol.Sun, Rank.Ten);
        public static Card JackOfSun => new(Symbol.Sun, Rank.Jack);
        public static Card QueenOfSun => new(Symbol.Sun, Rank.Queen);
        public static Card KingOfSun => new(Symbol.Sun, Rank.King);
        public static Card AceOfSun => new(Symbol.Sun, Rank.Ace);

        public static Card[] FullDeck => new[]
        {
            TwoOfMoon,
            ThreeOfMoon,
            FourOfMoon,
            FiveOfMoon,
            SixOfMoon,
            SevenOfMoon,
            EightOfMoon,
            NineOfMoon,
            TenOfMoon,
            JackOfMoon,
            QueenOfMoon,
            KingOfMoon,
            AceOfMoon,

            TwoOfSun,
            ThreeOfSun,
            FourOfSun,
            FiveOfSun,
            SixOfSun,
            SevenOfSun,
            EightOfSun,
            NineOfSun,
            TenOfSun,
            JackOfSun,
            QueenOfSun,
            KingOfSun,
            AceOfSun
        };

        public Card(Symbol symbol, Rank rank)
        {
            Symbol = symbol;
            Rank = rank;
        }

        public Rank Rank { get; }
        public Symbol Symbol { get; }
        public bool IsWild() => Rank is Rank.Jack && Symbol is Symbol.Sun;
        public bool IsRemover() => Rank is Rank.Jack && Symbol is Symbol.Moon;

        public bool Equals(Card other)
        {
            return Rank == other.Rank && Symbol == other.Symbol;
        }

        public override bool Equals(object obj)
        {
            return obj is Card other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Symbol, (int)Rank);
        }

        public override string ToString()
        {
            return $"{Rank} of {Symbol}";
        }
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

    public static class EnumExtensions
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
    }
}