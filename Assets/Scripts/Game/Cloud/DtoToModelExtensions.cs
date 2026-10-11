using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models;
using Game.Domain.Models.Dto;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.Organisms.Atoms;
using Game.Domain.Models.Organisms.Atoms.Molecules;
using Dto = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models;

namespace Game.Cloud
{
    public static class DtoToModelExtensions
    {
        public static Rank ToModel(this Dto.Rank rank)
        {
            return rank switch
            {
                Dto.Rank.Ace => Rank.Ace,
                Dto.Rank.Two => Rank.Two,
                Dto.Rank.Three => Rank.Three,
                Dto.Rank.Four => Rank.Four,
                Dto.Rank.Five => Rank.Five,
                Dto.Rank.Six => Rank.Six,
                Dto.Rank.Seven => Rank.Seven,
                Dto.Rank.Eight => Rank.Eight,
                Dto.Rank.Nine => Rank.Nine,
                Dto.Rank.Ten => Rank.Ten,
                Dto.Rank.Jack => Rank.Jack,
                Dto.Rank.Queen => Rank.Queen,
                Dto.Rank.King => Rank.King,
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        public static Symbol ToModel(this Dto.Symbol symbol)
        {
            return symbol switch
            {
                Dto.Symbol.Moon => Symbol.Moon,
                Dto.Symbol.Sun => Symbol.Sun,
                _ => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null)
            };
        }

        public static Card ToModel(this Dto.Card card)
        {
            return new Card(card.Symbol.ToModel(), card.Rank.ToModel());
        }

        public static Row ToModel(this Dto.Row row)
        {
            return row switch
            {
                Dto.Row.One => Row.One,
                Dto.Row.Two => Row.Two,
                Dto.Row.Three => Row.Three,
                Dto.Row.Four => Row.Four,
                Dto.Row.Five => Row.Five,
                Dto.Row.Six => Row.Six,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
            };
        }

        public static Column ToModel(this Dto.Column column)
        {
            return column switch
            {
                Dto.Column.One => Column.One,
                Dto.Column.Two => Column.Two,
                Dto.Column.Three => Column.Three,
                Dto.Column.Four => Column.Four,
                Dto.Column.Five => Column.Five,
                Dto.Column.Six => Column.Six,
                Dto.Column.Seven => Column.Seven,
                Dto.Column.Eight => Column.Eight,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
            };
        }

        public static Position ToModel(
            this Dto.Position position)
        {
            return new Position(position.Row.ToModel(), position.Column.ToModel());
        }

        public static Team ToModel(this Dto.Team team)
        {
            return team switch
            {
                Dto.Team.Red => Team.Red,
                Dto.Team.Yellow => Team.Yellow,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            };
        }

        public static Move ToModel(this Dto.Move move)
        {
            Card card = move.Card.ToModel();
            Position position = move.Position.ToModel();
            Team team = move.Team.ToModel();
            return new Move(position, card, team);
        }

        private static Card[] ToModel(this List<Dto.Card> cards)
        {
            return cards.Select(ToModel).ToArray();
        }

        private static Move[] ToModel(this List<Dto.Move> moves)
        {
            return moves.Select(ToModel).ToArray();
        }

        private static Position[] ToModel(this List<Dto.Position> moves)
        {
            return moves.Select(ToModel).ToArray();
        }

        public static Score ToModel(this Dto.Score score)
        {
            return new Score(score.Red, score.Yellow);
        }

        public static ClientGameState ToModel(this Dto.ClientGameState clientGameState)
        {
            return new ClientGameState(
                clientGameState.Moves.ToModel(),
                clientGameState.Hand.ToModel(),
                clientGameState.Locked.ToModel(),
                clientGameState.Team.ToModel(),
                clientGameState.Score.ToModel(),
                clientGameState.IsMyTurn);
        }

        public static Match ToModel(this Dto.Dto.Match match)
        {
            return new Match
            {
                Id = match.Id,
                ClientGameState = match.ClientGameState.ToModel()
            };
        }
    }
}