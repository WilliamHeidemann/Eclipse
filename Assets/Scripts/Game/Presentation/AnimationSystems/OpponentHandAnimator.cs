using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Game.Presentation.AnimationSystems
{
    public class OpponentHandAnimator : MonoBehaviour
    {
        [SerializeField] private UIDocument _cardPrefab;
        [SerializeField] private DiscardPile _discardPile;
        [SerializeField] private CardSprites _cardSprites;

        public async Awaitable AnimatePlay(Card card)
        {
            Quaternion startingRotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0f, 360f)));

            UIDocument document = Instantiate(_cardPrefab, transform.position, startingRotation);

            VisualElement root = document.rootVisualElement;

            root.Q<VisualElement>("Card").style.backgroundImage = new StyleBackground(_cardSprites.GetHandSprite(card));

            root.Q<Label>().text = card.Rank.AsSingleDigit();

            await _discardPile.Discard(document.transform);
        }
    }
}