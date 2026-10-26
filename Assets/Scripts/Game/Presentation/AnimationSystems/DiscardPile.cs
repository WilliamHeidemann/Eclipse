using System.Threading;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Presentation.AnimationSystems
{
    public class DiscardPile : MonoBehaviour
    {
        [SerializeField] private UIDocument _discardPile;
        [SerializeField] private float _discardDuration = 0.5f;

        private int _discards;

        private void OnEnable()
        {
            _discardPile.rootVisualElement.Q<VisualElement>("Card").style.display = DisplayStyle.None;
        }

        public async Awaitable Discard(Transform card, CancellationToken token)
        {
            _discards++;

            var targetPosition = _discardPile.transform.position +
                                 new Vector3(0f, Random.Range(-0.05f, 0.05f), -_discards * 0.01f);

            LMotion.Create(card.position, targetPosition, _discardDuration)
                .WithEase(Ease.InOutCubic)
                .BindToPosition(card)
                .RegisterTo(token);

            var targetRotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f)) * _discardPile.transform.rotation;

            await LMotion.Create(card.rotation, targetRotation, _discardDuration)
                .WithEase(Ease.InOutCubic)
                .BindToRotation(card)
                .ToAwaitable(token);
        }
    }
}
