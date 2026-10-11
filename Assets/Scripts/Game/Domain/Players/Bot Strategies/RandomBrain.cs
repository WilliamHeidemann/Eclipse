using System;
using System.Linq;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.Organisms.Atoms;
using Game.Domain.Models.Organisms.Atoms.Molecules;
using UtilityToolkit.CollectionExtensions;

namespace Game.Domain.Players.Bot_Strategies
{
    public class RandomBrain : IBrain
    {
        private readonly Random _random = new();

        public Move DecideMove(ClientGameState clientGameState)
        {
            Board board = new(clientGameState.Moves);

            Card card = clientGameState.Hand.Where(card => card.Rank != Rank.Jack).RandomElement();
            (Position first, Position second) = BoardLayout.Get(card);

            if (_random.NextDouble() < 0.5f) (first, second) = (second, first);

            Position position = board.Fits(first) ? first : second;

            Move move = new(position, card, clientGameState.Team);

            return move;
        }
    }
}