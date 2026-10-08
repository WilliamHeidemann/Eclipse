using System;
using System.Linq;

namespace Game.Domain.Models
{
    [Serializable]
    public class GameState
    {
        public GameState(Card[] redHand, Card[] yellowHand, Card[] deck, Move[] moves, Position[] locked, Score score,
            Team toPlay)
        {
            RedHand = redHand;
            YellowHand = yellowHand;
            Deck = deck;
            Moves = moves;
            Locked = locked;
            Score = score;
            ToPlay = toPlay;
        }

        public Card[] RedHand { get; }
        public Card[] YellowHand { get; }
        public Card[] Deck { get; }
        public Move[] Moves { get; }
        public Position[] Locked { get; }
        public Score Score { get; }
        public Team ToPlay { get; }

        public ClientGameState ToClientGameState(Team team)
        {
            Card[] hand = team == Team.Red ? RedHand : YellowHand;
            bool isMyTurn = team == ToPlay;

            return new ClientGameState
            {
                Moves = Moves,
                Hand = hand,
                Team = team,
                IsMyTurn = isMyTurn,
                Locked = Locked,
                Score = Score,
            };
        }

        public static GameState CreateInitial()
        {
            Deck deck = new();

            Card[] redCards = Enumerable.Range(0, 7).Select(_ => deck.Draw()).ToArray();

            Card[] yellowCards = Enumerable.Range(0, 7).Select(_ => deck.Draw()).ToArray();

            Move[] moves = Array.Empty<Move>();

            Position[] locked = Array.Empty<Position>();

            Score score = new();

            Random random = new();
            Team toPlay = random.NextDouble() < 0.5 ? Team.Red : Team.Yellow;

            return new GameState(redCards, yellowCards, deck.GetCards(), moves, locked, score, toPlay);
        }
    }
}