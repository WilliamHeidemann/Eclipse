using System;
using System.Linq;
using Game.Domain.Models.Organisms;

namespace Game.Domain.Models.State
{
    [Serializable]
    public record GameState(
        Card[] RedHand,
        Card[] YellowHand,
        Card[] Deck,
        Move[] Moves,
        Sequence[] Sequences,
        Team ToPlay)
    {
        public ClientGameState ToClientGameState(Team team)
        {
            Card[] hand = team == Team.Red ? RedHand : YellowHand;
            bool isMyTurn = team == ToPlay;

            return new ClientGameState(Moves, hand, Sequences, team, isMyTurn);
        }

        public static GameState CreateInitial()
        {
            Deck deck = new();

            Card[] redCards = Enumerable.Range(0, 7).Select(_ => deck.Draw()).ToArray();

            Card[] yellowCards = Enumerable.Range(0, 7).Select(_ => deck.Draw()).ToArray();

            Move[] moves = Array.Empty<Move>();

            Sequence[] sequences = Array.Empty<Sequence>();

            Random random = new();
            Team toPlay = random.NextDouble() < 0.5 ? Team.Red : Team.Yellow;

            return new GameState(redCards, yellowCards, deck.GetCards(), moves, sequences, toPlay);
        }
    }
}