using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace Game.Presentation.Timeline;

[TrackColor(0.2f, 0.7f, 0.5f)]
[TrackClipType(typeof(SpriteFlipbookAsset))]
[TrackBindingType(typeof(UIDocument))]
public class SpriteFlipbookTrack : TrackAsset
{
    [Tooltip("The name of the VisualElement inside the UI Document to target")]
    public string TargetElementName;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var playable = ScriptPlayable<SpriteFlipbookMixerBehaviour>.Create(graph, inputCount);
        playable.GetBehaviour().TargetElementName = TargetElementName;
        return playable;
    }
}