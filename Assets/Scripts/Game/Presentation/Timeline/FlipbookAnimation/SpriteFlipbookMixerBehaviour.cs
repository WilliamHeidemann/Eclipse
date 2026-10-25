using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace Game.Presentation.Timeline;

public class SpriteFlipbookMixerBehaviour : PlayableBehaviour
{
    public string TargetElementName;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // The bound UIDocument is delivered as playerData, not via the director's GameObject
        if (playerData is not UnityEngine.UIElements.UIDocument uiDocument || uiDocument.rootVisualElement == null)
            return;

        VisualElement TargetElement = uiDocument.rootVisualElement.Q<VisualElement>(TargetElementName);
        if (TargetElement == null)
            return;

        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            float weight = playable.GetInputWeight(i);
            if (weight > 0f)
            {
                var inputPlayable = (ScriptPlayable<SpriteFlipbookBehaviour>)playable.GetInput(i);
                var behaviour = inputPlayable.GetBehaviour();

                behaviour.targetElement = TargetElement;
            }
        }
    }
}