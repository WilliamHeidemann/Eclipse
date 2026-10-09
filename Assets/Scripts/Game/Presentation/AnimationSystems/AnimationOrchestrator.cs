using System.Collections.Generic;
using Game.Domain;
using Game.Domain.Models;
using Game.Presentation.Audio;
using UnityEngine;
using UnityEngine.UIElements;
using Position = Game.Domain.Models.Position;

namespace Game.Presentation.AnimationSystems
{
    public class AnimationOrchestrator : MonoBehaviour
    {
        [SerializeField] private BoardPresenter _boardPresenter;
        [SerializeField] private DrawAnimator _drawAnimator;
        [SerializeField] private CardAligner _cardAligner;
        [SerializeField] private DiscardPile _discardPile;
        [SerializeField] private OpponentHandAnimator _opponentHandAnimator;
        [SerializeField] private AudioPlayer _audioPlayer;

        private readonly AnimationQueue _animationQueue = new();

        public void BindAnimations(PlayCoordinator playCoordinator)
        {
            playCoordinator.OnDrawCard += PlayDrawAnimation;
            playCoordinator.OnOpponentPlayed += AnimateOpponentPlay;
            playCoordinator.OnValidMoveRequest += PlayDiscardAndPinAnimation;
            playCoordinator.OnInvalidMoveRequest += PlayInvalidMove;
        }

        public void PlayInvalidMove(Position position)
        {
            _boardPresenter.Shake(position);
                _audioPlayer.Play(Sound.No); // No is for unsuccessful placement
        }

        public async Awaitable PlaySequenceCelebration()
        {
            Debug.Log("SEQUENCE!");
        }

        public void PlayDrawAnimation(Card card)
        {
            _animationQueue.Enqueue(Draw, card);
            return;

            async Awaitable Draw(Card c)
            {
                UIDocument cardUIDocument = _drawAnimator.InstantiateCardFaceDown();
                _audioPlayer.Play(Sound.DrawCard, .15f);
                await _drawAnimator.AnimateDrawing(c, cardUIDocument);
                _audioPlayer.Play(Sound.ToHand);
                _cardAligner.AddCard(c, cardUIDocument.transform);
                float buffer = Random.Range(0.18f, .32f);
                await Awaitable.WaitForSecondsAsync(buffer);
            }
        }

        public void PlayDrawAnimation(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                PlayDrawAnimation(card);
            }
        }

        public void PlayDiscardAndPinAnimation(Move move)
        {
            _animationQueue.Enqueue(Play);
            return;

            async Awaitable Play()
            {
                _boardPresenter.Pop(move.Position);
                _audioPlayer.Play(Sound.Pop);

                if (_cardAligner.RemoveCard(move.Card, out Transform cardTransform))
                {
                    _audioPlayer.Play(Sound.PutDown);
                    await _discardPile.Discard(cardTransform);
                }

                if (move.Card.IsRemover())
                {
                    await _boardPresenter.RemovePin(move.Position);
                }
                else
                {
                    await _boardPresenter.Pin(move.Position, move.Team);
                    _audioPlayer.Play(Sound.Yes); // Yes is for successful placement
                }
            }
        }

        private void AnimateOpponentPlay(Move move)
        {
            _animationQueue.Enqueue(Play);
            return;
            
            async Awaitable Play()
            {
                await Awaitable.WaitForSecondsAsync(1f); // simulate thinking time.
                _audioPlayer.Play(Sound.PutDown);
                await _opponentHandAnimator.AnimatePlay(move.Card);
                if (move.Card.IsRemover())
                {
                    await _boardPresenter.RemovePin(move.Position);
                }
                else
                {
                    await _boardPresenter.Pin(move.Position, move.Team);
                }
            }
        }
    }
}