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

    public VisualElement TargetElement;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (TargetElement == null || Sprites == null || Sprites.Length == 0)
            return;

        double currentTime = playable.GetTime();
        double duration = playable.GetDuration();

        if (duration <= 0) return;
        float progress = Mathf.Clamp01((float)(currentTime / duration));
        int spriteIndex = Mathf.FloorToInt(progress * Sprites.Length);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, Sprites.Length - 1);
        TargetElement.style.backgroundImage = new StyleBackground(Sprites[spriteIndex]);
    }
}