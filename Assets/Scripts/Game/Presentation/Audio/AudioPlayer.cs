using FMODUnity;
using UnityEngine;

namespace Game.Presentation.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioContainer _audioContainer;

        public void Play(Sound sound)
        {
            EventReference eventRef = _audioContainer.Get(sound);
            if (eventRef.IsNull) return;

            // For UI / 2D audio:
            RuntimeManager.PlayOneShot(eventRef);

            // Alternatively, for 3D positional audio anchored to this object:
            // RuntimeManager.PlayOneShotAttached(eventRef, gameObject);
        }

        public async Awaitable Play(Sound sound, float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);
            Play(sound);
        }
    }
}