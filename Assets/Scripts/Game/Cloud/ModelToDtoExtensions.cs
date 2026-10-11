using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models;
using Game.Domain.Models.Organisms.Atoms;
using Game.Domain.Models.Organisms.Atoms.Molecules;
using Dto = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models;

namespace Game.Cloud
{
    public static class ModelToDtoExtensions
    {
        public static Dto.Rank ToDto(this Rank rank)
        {
            return rank switch
            {
                Rank.Ace => Dto.Rank.Ace,
                Rank.Two => Dto.Rank.Two,
                Rank.Three => Dto.Rank.Three,
                Rank.Four => Dto.Rank.Four,
                Rank.Five => Dto.Rank.Five,
                Rank.Six => Dto.Rank.Six,
                Rank.Seven => Dto.Rank.Seven,
                Rank.Eight => Dto.Rank.Eight,
                Rank.Nine => Dto.Rank.Nine,
                Rank.Ten => Dto.Rank.Ten,
                Rank.Jack => Dto.Rank.Jack,
                Rank.Queen => Dto.Rank.Queen,
                Rank.King => Dto.Rank.King,
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        public static Dto.Symbol ToDto(this Symbol symbol)
        {
            return symbol switch
            {
                Symbol.Moon => Dto.Symbol.Moon,
                Symbol.Sun => Dto.Symbol.Sun,
                _ => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null)
            };
        }

        public static Dto.Card ToDto(this Card card)
        {
            return new Dto.Card
            {
                Rank = card.Rank.ToDto(),
                Symbol = card.Symbol.ToDto()
            };
        }

        public static Dto.Row ToDto(this Row row)
        {
            return row switch
            {
                Row.One => Dto.Row.One,
                Row.Two => Dto.Row.Two,
                Row.Three => Dto.Row.Three,
                Row.Four => Dto.Row.Four,
                Row.Five => Dto.Row.Five,
                Row.Six => Dto.Row.Six,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
            };
        }

        public static Dto.Column ToDto(this Column column)
        {
            return column switch
            {
                Column.One => Dto.Column.One,
                Column.Two => Dto.Column.Two,
                Column.Three => Dto.Column.Three,
                Column.Four => Dto.Column.Four,
                Column.Five => Dto.Column.Five,
                Column.Six => Dto.Column.Six,
                Column.Seven => Dto.Column.Seven,
                Column.Eight => Dto.Column.Eight,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
            };
        }

        public static Dto.Position ToDto(this Position position)
        {
            return new Dto.Position
            {
                Row = position.Row.ToDto(),
                Column = position.Column.ToDto()
            };
        }

        public static Dto.Team ToDto(this Team team)
        {
            return team switch
            {
                Team.Red => Dto.Team.Red,
                Team.Yellow => Dto.Team.Yellow,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            };
        }

        public static Dto.Move ToDto(this Move move)
        {
            return new Dto.Move
            {
                Card = move.Card.ToDto(),
                Position = move.Position.ToDto(),
                Team = move.Team.ToDto()
            };
        }

        private static List<Dto.Card> ToDto(this Card[] cards)
        {
            return cards.Select(card => card.ToDto()).ToList();
        }
        
        private static List<Dto.Move> ToDto(this Move[] moves)
        {
            return moves.Select(move => move.ToDto()).ToList();
        }

        public static Dto.ClientGameState ToDto(this ClientGameState clientGameState)
        {
            return new Dto.ClientGameState
            {
                Team = clientGameState.Team.ToDto(),
                Hand = clientGameState.Hand.ToDto(),
                Moves = clientGameState.Moves.ToDto(),
                IsMyTurn = clientGameState.IsMyTurn
            };
        }
    }
}