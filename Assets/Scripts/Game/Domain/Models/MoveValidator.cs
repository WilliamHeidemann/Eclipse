using System;
using System.Collections.Generic;
using System.Linq;
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

            return IsValid(move, board, hand, toPlay) ? Option<Move>.Some(move) : Option<Move>.None;
        }

        public static bool IsValid(Move move, Board board, Hand hand, Team toPlay)
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

            if (!cardInHand.IsRemover())
            {
                return true;
            }

            bool playerOwnsPosition = board.OwnerIs(move.Position, move.Team);

            return !playerOwnsPosition;
        }

        public abstract class MoveResult
        {
            public class Success : MoveResult
            {
                public GameState UpdatedState { get; }
                public int DeltaScore { get; }

                public Success(GameState updatedState, int deltaScore)
                {
                    UpdatedState = updatedState;
                    DeltaScore = deltaScore;
                }

                public void Deconstruct(out GameState updatedState, out int deltaScore)
                {
                    updatedState = UpdatedState;
                    deltaScore = DeltaScore;
                }
            }

            public class Invalid : MoveResult
            {
            }

            public class OutOfSync : MoveResult
            {
            }
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

            if (!IsValid(move, board, hand, gameState.ToPlay))
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

    public static class SequenceExtension
    {
        public static IEnumerable<TResult> CombineAll<T, TResult>(
            this IEnumerable<T> source, Func<T, T, TResult> combinator)
        {
            IEnumerable<T> enumerable = source as T[] ?? source.ToArray();

            foreach (T item1 in enumerable)
            {
                foreach (T item2 in enumerable)
                {
                    yield return combinator(item1, item2);
                }
            }
        }
        
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey>? comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return Iterator();

            IEnumerable<TSource> Iterator()
            {
                var knownKeys = new HashSet<TKey>(comparer);
                foreach (TSource element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                    {
                        yield return element;
                    }
                }
            }
        }
    }
}