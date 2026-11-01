using System;
using System.Threading;
using FMODUnity;
using UnityEngine;

namespace Game.Presentation.Audio
{
    public class FmodAudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioContainer _audioContainer;
        [SerializeField] private UIAudioConfig _uiAudioConfig;
        
        public bool IsEnabled { get; private set; } = true;

        public void Play(Sound sound)
        {
            if (_audioContainer == null) return;
            Play(_audioContainer.Get(sound));
        }

        public void Play(UISound uiSound)
        {
            if (_uiAudioConfig == null) return;
            Play(_uiAudioConfig.Get(uiSound));
        }

        public void Play(EventReference eventRef)
        {
            if (!IsEnabled || eventRef.IsNull) return;
            RuntimeManager.PlayOneShot(eventRef);
        }

        public async Awaitable PlayAsync(Sound sound, float delaySeconds, CancellationToken cancellationToken = default)
        {
            if (_audioContainer == null) return;
            EventReference eventRef = _audioContainer.Get(sound);
            if (eventRef.IsNull) return;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                destroyCancellationToken,
                cancellationToken
            );

            try
            {
                await Awaitable.WaitForSecondsAsync(delaySeconds, cancellationToken: linkedCts.Token);
                if (IsEnabled) RuntimeManager.PlayOneShot(eventRef);
            }
            catch (OperationCanceledException)
            {
                // Clean exit if canceled or GameObject destroyed
            }
        }

        public void PlayAttached(Sound sound, GameObject target)
        {
            if (!IsEnabled || _audioContainer == null || target == null) return;
            EventReference eventRef = _audioContainer.Get(sound);
            if (eventRef.IsNull) return;

            RuntimeManager.PlayOneShotAttached(eventRef, target);
        }
        
        public bool Toggle()
        {
            IsEnabled = !IsEnabled;
            return IsEnabled;
        }
    }
}