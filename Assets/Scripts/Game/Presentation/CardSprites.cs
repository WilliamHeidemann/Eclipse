using System;
using Game.Domain.Models;
using Game.Domain.Models.Organisms.Atoms;
using Game.Domain.Models.Organisms.Atoms.Molecules;
using UnityEngine;

namespace Game.Presentation
{
    [CreateAssetMenu(fileName = "CardSprites", menuName = "CardSprites")]
    public class CardSprites : ScriptableObject
    {
        [SerializeField] private Sprite _moon;
        [SerializeField] private Sprite _sun;
        [SerializeField] private Sprite _wild;
        [SerializeField] private Sprite _remove;
        [SerializeField] private Sprite _moonBoard;
        [SerializeField] private Sprite _sunBoard;
        [SerializeField] private Sprite _cardBack;

        public Sprite CardBack => _cardBack;

        public Sprite GetBoardSprite(Symbol symbol)
        {
            return symbol switch
            {
                Symbol.Sun => _sunBoard,
                Symbol.Moon => _moonBoard,
                _ => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null)
            };
        }
        
        public Sprite GetHandSprite(Card card)
        {
            return (card.Symbol, card.Rank) switch
            {
                (Symbol.Moon, Rank.Jack) => _remove,
                (Symbol.Sun, Rank.Jack) => _wild,
                (Symbol.Moon, _) => _moon,
                (Symbol.Sun, _) => _sun,
                _ => throw new ArgumentException($"No sprite found for card: {card.Symbol} {card.Rank}")
            };
        }
    }
}