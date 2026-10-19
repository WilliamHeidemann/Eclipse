using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Presentation.AnimationSystems
{
    public class DrawAnimator : MonoBehaviour
    {
        [SerializeField] private UIDocument _drawPileDocument;
        [SerializeField] private CardSprites _cardSprites;
        [SerializeField] private UIDocument _cardPrefab;
        [SerializeField] private Transform _showDisplayTransform;
        [SerializeField] private Transform _discardPileTransform;

        [Header("Parameters")] [SerializeField]
        private float _durationToDisplay = 0.2f;

        [SerializeField] private float _displayDuration = 0.4f;

        public UIDocument InstantiateCardFaceDown()
        {
            return Instantiate(_cardPrefab);
        }
        
        public async Awaitable AnimateDrawing(Card card, UIDocument cardGameObject)
        {
            var cardTransform = cardGameObject.transform;

            cardTransform.SetPositionAndRotation(
                _drawPileDocument.transform.position,
                _drawPileDocument.transform.rotation);

            // Set position
            LMotion.Create(cardTransform.position, _showDisplayTransform.position, _durationToDisplay)
                .WithEase(Ease.InOutCubic)
                .BindToPosition(cardTransform);

            // Set rotation
            Quaternion halfWayRotation = Quaternion.Lerp(cardTransform.rotation, _showDisplayTransform.rotation, .5f) *
                                         Quaternion.Euler(0, -180, 0);

            // Set rotation
            LMotion.Create(cardTransform.rotation, halfWayRotation, _durationToDisplay / 2)
                .WithEase(Ease.InCubic)
                .WithOnComplete(() =>
                {
                    Sprite sprite = _cardSprites.GetHandSprite(card);
                    VisualElement root = cardGameObject.rootVisualElement;
                    root.Q<VisualElement>("Card").style.backgroundImage = new StyleBackground(sprite);
                    Label label = root.Q<Label>();
                    label.text = card.GetText();
                    label.EnableInClassList("card-rank--effect", card.IsWild() || card.IsRemover());
                    LMotion.Create(halfWayRotation, _showDisplayTransform.rotation, _durationToDisplay / 2)
                        .WithEase(Ease.OutCubic)
                        .BindToRotation(cardTransform);
                })
                .BindToRotation(cardTransform);

            // Set scale
            LMotion.Create(cardTransform.localScale, _showDisplayTransform.localScale, _durationToDisplay)
                .WithEase(Ease.InOutCubic)
                .BindToLocalScale(cardTransform);

            await Awaitable.WaitForSecondsAsync(_durationToDisplay + _displayDuration);
        }
    }
}