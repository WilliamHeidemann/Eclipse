using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models;
using Game.Domain.Models.Dto;
using Game.Domain.Models.Organisms;
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
    public static class DtoToModelExtensions
    {
        public static Rank ToModel(this Molecule.Rank rank)
        {
            return rank switch
            {
                Molecule.Rank.Ace => Rank.Ace,
                Molecule.Rank.Two => Rank.Two,
                Molecule.Rank.Three => Rank.Three,
                Molecule.Rank.Four => Rank.Four,
                Molecule.Rank.Five => Rank.Five,
                Molecule.Rank.Six => Rank.Six,
                Molecule.Rank.Seven => Rank.Seven,
                Molecule.Rank.Eight => Rank.Eight,
                Molecule.Rank.Nine => Rank.Nine,
                Molecule.Rank.Ten => Rank.Ten,
                Molecule.Rank.Jack => Rank.Jack,
                Molecule.Rank.Queen => Rank.Queen,
                Molecule.Rank.King => Rank.King,
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        public static Symbol ToModel(this Molecule.Symbol symbol)
        {
            return symbol switch
            {
                Molecule.Symbol.Moon => Symbol.Moon,
                Molecule.Symbol.Sun => Symbol.Sun,
                _ => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null)
            };
        }

        public static Card ToModel(this Atom.Card card)
        {
            return new Card(card.Symbol.ToModel(), card.Rank.ToModel());
        }

        public static Row ToModel(this Molecule.Row row)
        {
            return row switch
            {
                Molecule.Row.One => Row.One,
                Molecule.Row.Two => Row.Two,
                Molecule.Row.Three => Row.Three,
                Molecule.Row.Four => Row.Four,
                Molecule.Row.Five => Row.Five,
                Molecule.Row.Six => Row.Six,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
            };
        }

        public static Column ToModel(this Molecule.Column column)
        {
            return column switch
            {
                Molecule.Column.One => Column.One,
                Molecule.Column.Two => Column.Two,
                Molecule.Column.Three => Column.Three,
                Molecule.Column.Four => Column.Four,
                Molecule.Column.Five => Column.Five,
                Molecule.Column.Six => Column.Six,
                Molecule.Column.Seven => Column.Seven,
                Molecule.Column.Eight => Column.Eight,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
            };
        }

        public static Position ToModel(
            this Atom.Position position)
        {
            return new Position(position.Row.ToModel(), position.Column.ToModel());
        }

        public static Team ToModel(this Molecule.Team team)
        {
            return team switch
            {
                Molecule.Team.Red => Team.Red,
                Molecule.Team.Yellow => Team.Yellow,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            };
        }

        public static Move ToModel(this Atom.Move move)
        {
            Card card = move.Card.ToModel();
            Position position = move.Position.ToModel();
            Team team = move.Team.ToModel();
            return new Move(position, card, team);
        }

        private static Card[] ToModel(this List<Atom.Card> cards)
        {
            return cards.Select(ToModel).ToArray();
        }

        private static Move[] ToModel(this List<Atom.Move> moves)
        {
            return moves.Select(ToModel).ToArray();
        }

        private static Position[] ToModel(this List<Atom.Position> moves)
        {
            return moves.Select(ToModel).ToArray();
        }

        public static Score ToModel(this Organism.Score score)
        {
            return new Score(score.Red, score.Yellow);
        }

        public static ClientGameState ToModel(this State.ClientGameState clientGameState)
        {
            return new ClientGameState(
                clientGameState.Moves.ToModel(),
                clientGameState.Hand.ToModel(),
                clientGameState.Locked.ToModel(),
                clientGameState.Team.ToModel(),
                clientGameState.Score.ToModel(),
                clientGameState.IsMyTurn);
        }

        public static Match ToModel(this Dto.Match match)
        {
            return new Match
            {
                Id = match.Id,
                ClientGameState = match.ClientGameState.ToModel()
            };
        }
    }
}