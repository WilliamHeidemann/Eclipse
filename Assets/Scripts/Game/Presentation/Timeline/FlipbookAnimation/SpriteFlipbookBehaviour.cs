using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace Game.Presentation.Timeline;

[Serializable]
public class SpriteFlipbookBehaviour : PlayableBehaviour
{
    public Sprite[] Sprites;
    private const float FrameRate = 12f;

    // Internal reference to the target VisualElement bound via Timeline
    [HideInInspector] public VisualElement targetElement;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (targetElement == null || Sprites == null || Sprites.Length == 0)
            return;

        // Get the current local time and the total duration of the playable
        double currentTime = playable.GetTime();
        double duration = playable.GetDuration();

        if (duration <= 0) return;

        // Calculate normalized progress (0.0 to 1.0)
        float progress = Mathf.Clamp01((float)(currentTime / duration));

        // Map progress to an array index
        int spriteIndex = Mathf.FloorToInt(progress * Sprites.Length);
        
        // Ensure we don't go out of bounds on the very last frame (when progress == 1.0)
        spriteIndex = Mathf.Clamp(spriteIndex, 0, Sprites.Length - 1);

        // Apply the sprite
        targetElement.style.backgroundImage = new StyleBackground(Sprites[spriteIndex]);
    }
}