using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.Organisms.Atoms;
using Game.Domain.Models.Organisms.Atoms.Molecules;
using UtilityToolkit.Monads;

namespace Game.Domain.Models
{
    public static class MoveValidator
    {
        public static Option<Move> GetValidMove(ClientGameState clientGameState, Position position)
        {
            if (!clientGameState.IsMyTurn)
            {
                return Option<Move>.None;
            }

            Board board = new(clientGameState.Moves);

            Hand hand = new(clientGameState.Hand);

            Card tabbedCard = BoardLayout.Get(position);

            bool fits = board.Fits(position);

            Option<Card> requiredCard = hand.FindCard(tabbedCard, fits);

            if (!requiredCard.IsSome(out Card cardInHand))
            {
                return Option<Move>.None;
            }

            if (cardInHand.IsRemover())
            {
                bool ownerIsPlayer = board.OwnerIs(position, clientGameState.Team);

                if (ownerIsPlayer)
                {
                    return Option<Move>.None;
                }
            }

            Move move = new(position, cardInHand, clientGameState.Team);

            Team toPlay = clientGameState.IsMyTurn ? clientGameState.Team : clientGameState.Team.Opposing();

            return IsMoveValid(move, board, hand, clientGameState.Locked.ToHashSet(), toPlay)
                ? Option<Move>.Some(move)
                : Option<Move>.None;
        }

        public static bool IsMoveValid(Move move, Board board, Hand hand, HashSet<Position> locked, Team toPlay)
        {
            if (move.Team != toPlay)
            {
                return false;
            }

            bool isOpen = board.Fits(move.Position);

            var requiredCard = hand.FindCard(move.Card, isOpen);

            if (!requiredCard.IsSome(out Card cardInHand))
            {
                return false;
            }

            if (cardInHand.Rank == Rank.Jack)
            {
                board.TryAdd(move.Position, move.Team);
                var sequences = SequencePatterns.FindSequences(board, move.Team, locked).ToArray();
                int deltaScore = sequences.Length;
                if (deltaScore > 0)
                {
                    return false;
                }
            }

            if (cardInHand.IsRemover())
            {
                bool playerOwnsPosition = board.OwnerIs(move.Position, move.Team);

                return !playerOwnsPosition && !locked.Contains(move.Position);
            }

            return true;
        }

        public static MoveResult PlayMove(GameState gameState, Move move)
        {
            Card[] cardsInHand = move.Team switch
            {
                Team.Red => gameState.RedHand,
                Team.Yellow => gameState.YellowHand,
                _ => throw new ArgumentOutOfRangeException()
            };

            Hand hand = new(cardsInHand);
            Board board = new(gameState.Moves);

            if (!IsMoveValid(move, board, hand, gameState.Locked.ToHashSet(), gameState.ToPlay))
            {
                return new MoveResult.Invalid();
            }

            if (move.Card.IsRemover()) board.Remove(move.Position);
            else board.TryAdd(move.Position, move.Team);

            hand.TryRemove(move.Card);

            Deck deck = new(gameState.Deck);
            Card draw = deck.Draw();

            hand.TryAdd(draw);

            MoveHistory moveHistory = new(gameState.Moves);
            moveHistory.Add(move);

            HashSet<Position> locked = new(gameState.Locked);
            var sequences = SequencePatterns.FindSequences(board, move.Team, locked).ToArray();
            int deltaScore = sequences.Length;

            if (move.Card.Rank == Rank.Jack && deltaScore > 0)
            {
                return new MoveResult.Invalid();
            }

            (Card[] redHand, Card[] yellowHand) = move.Team switch
            {
                Team.Red => (hand.GetCards(), gameState.YellowHand),
                Team.Yellow => (gameState.RedHand, hand.GetCards()),
                _ => throw new ArgumentOutOfRangeException()
            };

            GameState updatedGameState = new(
                redHand, yellowHand,
                deck.GetCards(), moveHistory.GetMoves(), locked.ToArray(),
                gameState.Score, move.Team.Opposing());

            return new MoveResult.Success(updatedGameState, deltaScore);
        }
    }
}