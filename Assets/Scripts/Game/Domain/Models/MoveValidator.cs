using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using UtilityToolkit.Monads;

namespace Game.Domain.Models
{
    public static class MoveValidator
    {
        public static Option<Play> GetValidPlay(ClientGameState clientGameState, Position position)
        {
            if (!clientGameState.IsMyTurn)
            {
                return Option<Play>.None;
            }

            Board board = new(clientGameState.Moves);

            Hand hand = new(clientGameState.Hand);

            Card tabbedCard = BoardLayout.Get(position);

            bool fits = board.Fits(position);

            Option<Card> requiredCard = hand.FindCard(tabbedCard, fits);

            if (!requiredCard.IsSome(out Card cardInHand))
            {
                return Option<Play>.None;
            }

            if (cardInHand.IsRemover())
            {
                bool ownerIsPlayer = board.OwnerIs(position, clientGameState.Team);

                if (ownerIsPlayer)
                {
                    return Option<Play>.None;
                }
            }

            Move move = new(position, clientGameState.Team);

            Team toPlay = clientGameState.IsMyTurn ? clientGameState.Team : clientGameState.Team.Opposing();

            return IsMoveValid(move, board, hand, clientGameState.Sequences, toPlay)
                ? Option<Play>.Some(new Play(move, cardInHand))
                : Option<Play>.None;
        }

        public static bool IsMoveValid(Move move, Board board, Hand hand, Sequence[] sequences, Team toPlay)
        {
            if (move.Team != toPlay)
            {
                return false;
            }

            bool isOpen = board.Fits(move.Position);

            Card requiredCard = isOpen ? BoardLayout.Get(move.Position) : Card.JackOfMoon;

            bool hasCardInHand = hand.FindCard(requiredCard, isOpen).IsSome(out _);

            if (!hasCardInHand)
            {
                return false;
            }

            if (requiredCard == Card.JackOfSun)
            {
                board.TryAdd(move.Position, move.Team);
                var deltaScore = SequencePatterns.FindSequences(board, move.Team, sequences).ToArray().Length;
                board.Remove(move.Position);
                if (deltaScore > 0)
                {
                    return false;
                }
            }
            else if (requiredCard == Card.JackOfMoon)
            {
                bool playerOwnsPosition = board.OwnerIs(move.Position, move.Team);

                return !playerOwnsPosition && !sequences.Any(s => s.Positions.Contains(move.Position));
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

            if (!IsMoveValid(move, board, hand, gameState.Sequences, gameState.ToPlay))
            {
                return new MoveResult.Invalid();
            }

            if (board.Fits(move.Position)) board.TryAdd(move.Position, move.Team);
            else board.Remove(move.Position);

            Card requiredCard = board.Fits(move.Position) ? Card.JackOfMoon : BoardLayout.Get(move.Position);

            hand.TryRemove(requiredCard);

            Deck deck = new(gameState.Deck);
            Card draw = deck.Draw();

            hand.TryAdd(draw);

            MoveHistory moveHistory = new(gameState.Moves);
            moveHistory.Add(move);

            var sequences = SequencePatterns.FindSequences(board, move.Team, gameState.Sequences).ToArray();
            int deltaScore = sequences.Length;

            if (requiredCard == Card.JackOfSun && deltaScore > 0)
            {
                return new MoveResult.Invalid();
            }

            var updatedSequences = gameState.Sequences.Concat(sequences).ToArray();

            (Card[] redHand, Card[] yellowHand) = move.Team switch
            {
                Team.Red => (hand.GetCards(), gameState.YellowHand),
                Team.Yellow => (gameState.RedHand, hand.GetCards()),
                _ => throw new ArgumentOutOfRangeException()
            };

            GameState updatedGameState = new(
                redHand, yellowHand,
                deck.GetCards(), moveHistory.GetMoves(),
                updatedSequences, move.Team.Opposing());

            return new MoveResult.Success(updatedGameState);
        }
    }
}