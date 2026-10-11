using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models;
using Game.Domain.Models.Organisms.Atoms;
using Game.Domain.Models.Organisms.Atoms.Molecules;
using Game.Domain.Models.State;
using Molecule = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.Organisms.Atoms.Molecules;
using Atom = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.Organisms.Atoms;
using Organism = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.Organisms;
using Dto = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.Dto;
using State = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.State;

namespace Game.Cloud
{
    public static class ModelToDtoExtensions
    {
        public static Molecule.Rank ToDto(this Rank rank)
        {
            return rank switch
            {
                Rank.Ace => Molecule.Rank.Ace,
                Rank.Two => Molecule.Rank.Two,
                Rank.Three => Molecule.Rank.Three,
                Rank.Four => Molecule.Rank.Four,
                Rank.Five => Molecule.Rank.Five,
                Rank.Six => Molecule.Rank.Six,
                Rank.Seven => Molecule.Rank.Seven,
                Rank.Eight => Molecule.Rank.Eight,
                Rank.Nine => Molecule.Rank.Nine,
                Rank.Ten => Molecule.Rank.Ten,
                Rank.Jack => Molecule.Rank.Jack,
                Rank.Queen => Molecule.Rank.Queen,
                Rank.King => Molecule.Rank.King,
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        public static Molecule.Symbol ToDto(this Symbol symbol)
        {
            return symbol switch
            {
                Symbol.Moon => Molecule.Symbol.Moon,
                Symbol.Sun => Molecule.Symbol.Sun,
                _ => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null)
            };
        }

        public static Atom.Card ToDto(this Card card)
        {
            return new Atom.Card
            {
                Rank = card.Rank.ToDto(),
                Symbol = card.Symbol.ToDto()
            };
        }

        public static Molecule.Row ToDto(this Row row)
        {
            return row switch
            {
                Row.One => Molecule.Row.One,
                Row.Two => Molecule.Row.Two,
                Row.Three => Molecule.Row.Three,
                Row.Four => Molecule.Row.Four,
                Row.Five => Molecule.Row.Five,
                Row.Six => Molecule.Row.Six,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
            };
        }

        public static Molecule.Column ToDto(this Column column)
        {
            return column switch
            {
                Column.One => Molecule.Column.One,
                Column.Two => Molecule.Column.Two,
                Column.Three => Molecule.Column.Three,
                Column.Four => Molecule.Column.Four,
                Column.Five => Molecule.Column.Five,
                Column.Six => Molecule.Column.Six,
                Column.Seven => Molecule.Column.Seven,
                Column.Eight => Molecule.Column.Eight,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
            };
        }

        public static Atom.Position ToDto(this Position position)
        {
            return new Atom.Position
            {
                Row = position.Row.ToDto(),
                Column = position.Column.ToDto()
            };
        }

        public static Molecule.Team ToDto(this Team team)
        {
            return team switch
            {
                Team.Red => Molecule.Team.Red,
                Team.Yellow => Molecule.Team.Yellow,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            };
        }

        public static Atom.Move ToDto(this Move move)
        {
            return new Atom.Move
            {
                Card = move.Card.ToDto(),
                Position = move.Position.ToDto(),
                Team = move.Team.ToDto()
            };
        }

        private static List<Atom.Card> ToDto(this Card[] cards)
        {
            return cards.Select(card => card.ToDto()).ToList();
        }
        
        private static List<Atom.Move> ToDto(this Move[] moves)
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