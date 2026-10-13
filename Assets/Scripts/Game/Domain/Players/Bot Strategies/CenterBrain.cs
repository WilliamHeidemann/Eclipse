using System;
using System.Linq;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using UtilityToolkit.CollectionExtensions;

namespace Game.Domain.Players.Bot_Strategies
{
    public class CenterBrain : IBrain
    {
        public Move DecideMove(ClientGameState clientGameState)
        {
            // cards in hand -> all positions on the board
            // order by distance to center
            // select first one

            var board = new Board(clientGameState.Moves);

            var position = clientGameState.Hand
                .Where(card => card.Rank != Rank.Jack)
                .SelectMany(card =>
                {
                    (Position first, Position second) = BoardLayout.Get(card);
                    return new[] { first, second };
                })
                .Where(board.Fits).OrderByDescending(position =>
                {
                    int row = (int)position.Row;
                    int column = (int)position.Column;

                    int rowScore = row switch
                    {
                        0 => 0,
                        1 => 1,
                        2 => 2,
                        3 => 2,
                        4 => 1,
                        5 => 0,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    int columnScore = column switch
                    {
                        0 => 0,
                        1 => 1,
                        2 => 2,
                        3 => 3,
                        4 => 3,
                        5 => 2,
                        6 => 1,
                        7 => 0,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    return rowScore * columnScore;
                }).FirstOption();

            if (position.IsSome(out Position playedPosition))
            {
                return new Move(playedPosition, BoardLayout.Get(playedPosition), clientGameState.Team);
            }

            throw new Exception("No room for any card");
        }
    }
}