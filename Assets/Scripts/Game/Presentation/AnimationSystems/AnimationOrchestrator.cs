using System;
using System.Threading;
using Game.Domain;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Presentation.Audio;
using Game.Presentation.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Position = Game.Domain.Models.Organisms.Position;
using Random = UnityEngine.Random;

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
        [SerializeField] private UIManager _uiManager;

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
            _animationQueue.Dispose();
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
            _animationQueue.Enqueue(ShowWinScreen);
            return;

            async Awaitable ShowWinScreen(CancellationToken token)
            {
                await Awaitable.NextFrameAsync(token);
                _uiManager.ShowGameOver(team);
            }
        }

        private void ShowWhoseTurnItIs(Team toPlay)
        {
            _uiManager.ShowTurn(toPlay);
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
            _animationQueue.Enqueue(token => Draw(card, token));
            return;

            async Awaitable Draw(Card c, CancellationToken token)
            {
                UIDocument cardUIDocument = _drawAnimator.InstantiateCardFaceDown();

                // Play delayed sound with cancellation token safety
                _ = _audioService.PlayAsync(Sound.DrawCard, 0.15f, token);

                cardUIDocument.transform.parent = _dynamicGameElements;
                await _drawAnimator.AnimateDrawing(c, cardUIDocument, token);
                _audioService.Play(Sound.ToHand);
                _cardAligner.AddCard(c, cardUIDocument.transform);
                // float buffer = Random.Range(0.09f, .16f);
                float buffer = Random.Range(0.09f, .16f);
                await Awaitable.WaitForSecondsAsync(buffer, token);
            }
        }

        public void PlayDiscardAndPinAnimation(Move move)
        {
            _boardPresenter.Pop(move.Position);
            _audioService.Play(Sound.Pop);

            _animationQueue.Enqueue(Play);
            return;

            async Awaitable Play(CancellationToken token)
            {
                if (_cardAligner.RemoveCard(move.Card, out Transform cardTransform))
                {
                    _audioService.Play(Sound.PutDown);
                    await _discardPile.Discard(cardTransform, token);
                }

                if (move.Card.IsRemover())
                {
                    await _boardPresenter.RemovePin(move.Position, token);
                }
                else
                {
                    var pin = await _boardPresenter.Pin(move.Position, move.Team, token);
                    pin.parent = _dynamicGameElements;
                    _audioService.Play(Sound.Yes); // Yes is for successful placement
                }
            }
        }

        private void AnimateOpponentPlay(Move move)
        {
            _animationQueue.Enqueue(Play);
            return;

            async Awaitable Play(CancellationToken token)
            {
                Card card = move.Card;

                var cardFromHand = await _opponentHandAnimator.AnimatePlay(card, token);
                cardFromHand.parent = _dynamicGameElements;
                
                if (card.IsRemover())
                {
                    await _boardPresenter.RemovePin(move.Position, token);
                }
                else
                {
                    var pin = await _boardPresenter.Pin(move.Position, move.Team, token);
                    pin.parent = _dynamicGameElements;
                    _audioService.Play(Sound.Click);
                }
            }
        }

        public void QueueDelay(float delayInSeconds)
        {
            _animationQueue.Enqueue(token => Wait(delayInSeconds, token));

            async Awaitable Wait(float delay, CancellationToken token)
            {
                await Awaitable.WaitForSecondsAsync(delay, token);
            }
        }
    }
}