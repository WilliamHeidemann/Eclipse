using System.Threading;
using FMODUnity;
using Game.Presentation.UI.Audio;
using UnityEngine;

namespace Game.Presentation.Audio
{
    public interface IAudioService
    {
        // Gameplay (Enum-based)
        void Play(Sound sound);
        Awaitable PlayAsync(Sound sound, float delaySeconds, CancellationToken cancellationToken = default);

        // UI (Enum-based)
        void Play(UISound uiSound);

        // Direct FMOD overrides (when designers assign custom events directly)
        void Play(EventReference eventRef);

        // 3D positional audio
        void PlayAttached(Sound sound, GameObject target);
    }
}