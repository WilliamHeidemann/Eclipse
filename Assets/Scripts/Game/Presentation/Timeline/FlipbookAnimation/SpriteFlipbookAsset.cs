using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace Game.Presentation.Timeline;

[Serializable]
public class SpriteFlipbookAsset : PlayableAsset, ITimelineClipAsset
{
    [SerializeField] public SpriteFlipbookBehaviour Template = new SpriteFlipbookBehaviour();

    public ClipCaps clipCaps => ClipCaps.All;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        Template ??= new SpriteFlipbookBehaviour();
        return ScriptPlayable<SpriteFlipbookBehaviour>.Create(graph, Template);
    }
}