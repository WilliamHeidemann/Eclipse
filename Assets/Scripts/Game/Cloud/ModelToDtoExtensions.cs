using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using Organism = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.Organisms;
using State = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.State;

namespace Game.Cloud
{
    public static class ModelToDtoExtensions
    {
        public static Organism.Rank ToDto(this Rank rank)
        {
            return rank switch
            {
                Rank.Ace => Organism.Rank.Ace,
                Rank.Two => Organism.Rank.Two,
                Rank.Three => Organism.Rank.Three,
                Rank.Four => Organism.Rank.Four,
                Rank.Five => Organism.Rank.Five,
                Rank.Six => Organism.Rank.Six,
                Rank.Seven => Organism.Rank.Seven,
                Rank.Eight => Organism.Rank.Eight,
                Rank.Nine => Organism.Rank.Nine,
                Rank.Ten => Organism.Rank.Ten,
                Rank.Jack => Organism.Rank.Jack,
                Rank.Queen => Organism.Rank.Queen,
                Rank.King => Organism.Rank.King,
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        public static Organism.Symbol ToDto(this Symbol symbol)
        {
            return symbol switch
            {
                Symbol.Moon => Organism.Symbol.Moon,
                Symbol.Sun => Organism.Symbol.Sun,
                _ => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null)
            };
        }

        public static Organism.Card ToDto(this Card card)
        {
            return new Organism.Card
            {
                Rank = card.Rank.ToDto(),
                Symbol = card.Symbol.ToDto()
            };
        }

        public static Organism.Row ToDto(this Row row)
        {
            return row switch
            {
                Row.One => Organism.Row.One,
                Row.Two => Organism.Row.Two,
                Row.Three => Organism.Row.Three,
                Row.Four => Organism.Row.Four,
                Row.Five => Organism.Row.Five,
                Row.Six => Organism.Row.Six,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
            };
        }

        public static Organism.Column ToDto(this Column column)
        {
            return column switch
            {
                Column.One => Organism.Column.One,
                Column.Two => Organism.Column.Two,
                Column.Three => Organism.Column.Three,
                Column.Four => Organism.Column.Four,
                Column.Five => Organism.Column.Five,
                Column.Six => Organism.Column.Six,
                Column.Seven => Organism.Column.Seven,
                Column.Eight => Organism.Column.Eight,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
            };
        }

        public static Organism.Position ToDto(this Position position)
        {
            return new Organism.Position
            {
                Row = position.Row.ToDto(),
                Column = position.Column.ToDto()
            };
        }

        public static Organism.Team ToDto(this Team team)
        {
            return team switch
            {
                Team.Red => Organism.Team.Red,
                Team.Yellow => Organism.Team.Yellow,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            };
        }

        public static Organism.Move ToDto(this Move move)
        {
            return new Organism.Move
            {
                Card = move.Card.ToDto(),
                Position = move.Position.ToDto(),
                Team = move.Team.ToDto()
            };
        }

        private static List<Organism.Card> ToDto(this Card[] cards)
        {
            return cards.Select(card => card.ToDto()).ToList();
        }
        
        private static List<Organism.Move> ToDto(this Move[] moves)
        {
            return moves.Select(move => move.ToDto()).ToList();
        }

        public static State.ClientGameState ToDto(this ClientGameState clientGameState)
        {
            return new State.ClientGameState
            {
                Team = clientGameState.Team.ToDto(),
                Hand = clientGameState.Hand.ToDto(),
                Moves = clientGameState.Moves.ToDto(),
                IsMyTurn = clientGameState.IsMyTurn
            };
        }
    }
}