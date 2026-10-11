using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models.Organisms.Atoms;
using UtilityToolkit.CollectionExtensions;

namespace Game.Domain.Models.Organisms
{
    public class Deck
    {
        private Stack<Card> Cards { get; set; } = new();

        public Deck()
        {
            Reshuffle();
        }

        public Deck(Card[] cards)
        {
            Cards = cards.ToStack();
        }
        
        public Card Draw()
        {
            if (Cards.Count == 0) Reshuffle();
            
            return Cards.Pop();
        }

        public void Reshuffle()
        { 
            Cards.Clear();

            Card[] doubleDeck = Card.FullDeck.Concat(Card.FullDeck).ToArray();

            foreach (Card card in doubleDeck.Shuffle())
            {
                Cards.Push(card);
            }
        }
        
        public Card[] GetCards()
        {
            return Cards.ToArray();
        }
    }
}