using System.Collections.Generic;
using System.Linq;
using UtilityToolkit.CollectionExtensions;
using UtilityToolkit.Monads;

namespace Game.Domain.Models.Organisms
{
    public class Hand
    {
        private List<Card> Cards { get; set; }

        public bool TryAdd(Card card)
        {
            if (IsFull)
                return false;

            Cards.Add(card);
            return true;
        }

        public bool TryRemove(Card card)
        {
            return Cards.Remove(card);
        }

        public bool Contains(Card card) => Cards.Contains(card);

        public int Count => Cards.Count;
        public bool IsFull => Count == 7;

        public Card[] GetCards() => Cards.ToArray();

        public override string ToString()
        {
            return string.Join(", ", GetCards());
        }

        public Hand(Card[] cards)
        {
            Cards = cards.ToList();
        }
        
        public Option<Card> FindCard(Card cardToCover, bool isOpenSpace)
        {
            if (isOpenSpace)
            {
                if (Contains(cardToCover)) return Option<Card>.Some(cardToCover);
                return Cards.FirstOption(c => c.IsWild());
            }

            return Cards.FirstOption(c => c.IsRemover());
        }
    }
}