using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models;
using Game.Domain.Models.Dto;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using Organism = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.Organisms;
using Dto = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.Dto;
using State = Unity.Services.CloudCode.GeneratedBindings.Game.Domain.Models.State;

namespace Game.Cloud
{
    public static class DtoToModelExtensions
    {
        public static Rank ToModel(this Organism.Rank rank)
        {
            return rank switch
            {
                Organism.Rank.Ace => Rank.Ace,
                Organism.Rank.Two => Rank.Two,
                Organism.Rank.Three => Rank.Three,
                Organism.Rank.Four => Rank.Four,
                Organism.Rank.Five => Rank.Five,
                Organism.Rank.Six => Rank.Six,
                Organism.Rank.Seven => Rank.Seven,
                Organism.Rank.Eight => Rank.Eight,
                Organism.Rank.Nine => Rank.Nine,
                Organism.Rank.Ten => Rank.Ten,
                Organism.Rank.Jack => Rank.Jack,
                Organism.Rank.Queen => Rank.Queen,
                Organism.Rank.King => Rank.King,
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        public static Symbol ToModel(this Organism.Symbol symbol)
        {
            return symbol switch
            {
                Organism.Symbol.Moon => Symbol.Moon,
                Organism.Symbol.Sun => Symbol.Sun,
                _ => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null)
            };
        }

        public static Card ToModel(this Organism.Card card)
        {
            return new Card(card.Symbol.ToModel(), card.Rank.ToModel());
        }

        public static Row ToModel(this Organism.Row row)
        {
            return row switch
            {
                Organism.Row.One => Row.One,
                Organism.Row.Two => Row.Two,
                Organism.Row.Three => Row.Three,
                Organism.Row.Four => Row.Four,
                Organism.Row.Five => Row.Five,
                Organism.Row.Six => Row.Six,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
            };
        }

        public static Column ToModel(this Organism.Column column)
        {
            return column switch
            {
                Organism.Column.One => Column.One,
                Organism.Column.Two => Column.Two,
                Organism.Column.Three => Column.Three,
                Organism.Column.Four => Column.Four,
                Organism.Column.Five => Column.Five,
                Organism.Column.Six => Column.Six,
                Organism.Column.Seven => Column.Seven,
                Organism.Column.Eight => Column.Eight,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
            };
        }

        public static Position ToModel(
            this Organism.Position position)
        {
            return new Position(position.Row.ToModel(), position.Column.ToModel());
        }

        public static Team ToModel(this Organism.Team team)
        {
            return team switch
            {
                Organism.Team.Red => Team.Red,
                Organism.Team.Yellow => Team.Yellow,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            };
        }

        public static Move ToModel(this Organism.Move move)
        {
            Position position = move.Position.ToModel();
            Team team = move.Team.ToModel();
            return new Move(position, team);
        }

        private static Card[] ToModel(this List<Organism.Card> cards)
        {
            return cards.Select(ToModel).ToArray();
        }

        private static Move[] ToModel(this List<Organism.Move> moves)
        {
            return moves.Select(ToModel).ToArray();
        }

        private static Position[] ToModel(this List<Organism.Position> moves)
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