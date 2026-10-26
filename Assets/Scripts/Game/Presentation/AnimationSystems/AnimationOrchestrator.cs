using System.Collections.Generic;
using Game.Domain;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Presentation.Audio;
using UnityEngine;
using UnityEngine.UIElements;
using Position = Game.Domain.Models.Organisms.Position;

namespace Game.Presentation.AnimationSystems
{
    public class AnimationOrchestrator : MonoBehaviour
    {
        [SerializeField] private BoardPresenter _boardPresenter;
        [SerializeField] private DrawAnimator _drawAnimator;
        [SerializeField] private CardAligner _cardAligner;
        [SerializeField] private DiscardPile _discardPile;
        [SerializeField] private OpponentHandAnimator _opponentHandAnimator;
        [SerializeField] private FmodAudioService _audioService;
        [SerializeField] private Transform _dynamicGameElements;
        [SerializeField] private GameObject _staticGameElements;

        private readonly AnimationQueue _animationQueue = new();
        private PlayCoordinator _playCoordinator;

        public void BindAnimations(PlayCoordinator playCoordinator)
        {
            _playCoordinator = playCoordinator;

            _playCoordinator.OnDrawCard += PlayDrawAnimation;
            _playCoordinator.OnOpponentPlayed += AnimateOpponentPlay;
            _playCoordinator.OnValidMoveRequest += PlayDiscardAndPinAnimation;
            _playCoordinator.OnInvalidMoveRequest += PlayInvalidMove;
            _playCoordinator.OnSequenceScored += PlaySequenceCelebration;
            _playCoordinator.OnTurnPassedTo += ShowWhoseTurnItIs;
            _playCoordinator.OnTeamWon += PlayWinAnimation;
        }

        private void OnDestroy()
        {
            if (_playCoordinator == null) return;

            _playCoordinator.OnDrawCard -= PlayDrawAnimation;
            _playCoordinator.OnOpponentPlayed -= AnimateOpponentPlay;
            _playCoordinator.OnValidMoveRequest -= PlayDiscardAndPinAnimation;
            _playCoordinator.OnInvalidMoveRequest -= PlayInvalidMove;
            _playCoordinator.OnSequenceScored -= PlaySequenceCelebration;
            _playCoordinator.OnTurnPassedTo -= ShowWhoseTurnItIs;
            _playCoordinator.OnTeamWon -= PlayWinAnimation;
        }

        public void ShowStaticGameElements()
        {
            _staticGameElements.SetActive(true);
        }

        public void HideGameElements()
        {
            _animationQueue.Clear();
            _cardAligner.Clear();
            _boardPresenter.Clear();
            _staticGameElements.SetActive(false);
            foreach (Transform element in _dynamicGameElements)
            {
                Destroy(element.gameObject);
            }
        }

        private void PlayWinAnimation(Team team)
        {
            Debug.Log($"{team} has won!");
        }

        private void ShowWhoseTurnItIs(Team toPlay)
        {
            Debug.Log($"To play: {toPlay}");
        }

        public void PlayInvalidMove(Position position)
        {
            _boardPresenter.Shake(position);
            _audioService.Play(Sound.No); // No is for unsuccessful placement
        }

        public void PlaySequenceCelebration(Sequence sequence)
        {
            Debug.Log("SEQUENCE!");
            foreach (var position in sequence.Positions)
            {
                _boardPresenter.Highlight(position);
            }
        }

        public void PlayDrawAnimation(Card card)
        {
            _animationQueue.Enqueue(Draw, card);
            return;

            async Awaitable Draw(Card c)
            {
                UIDocument cardUIDocument = _drawAnimator.InstantiateCardFaceDown();

                // Play delayed sound with cancellation token safety
                _ = _audioService.PlayAsync(Sound.DrawCard, 0.15f, destroyCancellationToken);

                cardUIDocument.transform.parent = _dynamicGameElements;
                await _drawAnimator.AnimateDrawing(c, cardUIDocument);
                _audioService.Play(Sound.ToHand);
                _cardAligner.AddCard(c, cardUIDocument.transform);
                // float buffer = Random.Range(0.09f, .16f);
                float buffer = Random.Range(0.09f, .16f);
                await Awaitable.WaitForSecondsAsync(buffer);
            }
        }

        public void PlayDiscardAndPinAnimation(Move move)
        {
            _boardPresenter.Pop(move.Position);
            _audioService.Play(Sound.Pop);

            _animationQueue.Enqueue(Play);
            return;

            async Awaitable Play()
            {
                if (_cardAligner.RemoveCard(move.Card, out Transform cardTransform))
                {
                    _audioService.Play(Sound.PutDown);
                    await _discardPile.Discard(cardTransform);
                }

                if (move.Card.IsRemover())
                {
                    await _boardPresenter.RemovePin(move.Position);
                }
                else
                {
                    var pin = await _boardPresenter.Pin(move.Position, move.Team);
                    pin.parent = _dynamicGameElements;
                    _audioService.Play(Sound.Yes); // Yes is for successful placement
                }
            }
        }

        private void AnimateOpponentPlay(Move move)
        {
            _animationQueue.Enqueue(Play);
            return;

            async Awaitable Play()
            {
                Card card = move.Card;

                var cardFromHand = await _opponentHandAnimator.AnimatePlay(card);
                cardFromHand.parent = _dynamicGameElements;
                
                if (card.IsRemover())
                {
                    await _boardPresenter.RemovePin(move.Position);
                }
                else
                {
                    var pin = await _boardPresenter.Pin(move.Position, move.Team);
                    pin.parent = _dynamicGameElements;
                    _audioService.Play(Sound.Click);
                }
            }
        }

        public void QueueDelay(float delayInSeconds)
        {
            _animationQueue.Enqueue(Wait, delayInSeconds);

            async Awaitable Wait(float delay)
            {
                await Awaitable.WaitForSecondsAsync(delay);
            }
        }
    }
}