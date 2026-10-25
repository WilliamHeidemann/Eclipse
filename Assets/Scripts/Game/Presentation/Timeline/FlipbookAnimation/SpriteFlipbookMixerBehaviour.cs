using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace Game.Presentation.Timeline;

public class SpriteFlipbookMixerBehaviour : PlayableBehaviour
{
    public string TargetElementName;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (playerData is not UIDocument uiDocument || uiDocument.rootVisualElement == null)
            return;

        VisualElement targetElement = uiDocument.rootVisualElement.Q<VisualElement>(TargetElementName);

        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            float weight = playable.GetInputWeight(i);
            if (weight > 0f)
            {
                var inputPlayable = (ScriptPlayable<SpriteFlipbookBehaviour>)playable.GetInput(i);
                var behaviour = inputPlayable.GetBehaviour();

                behaviour.TargetElement = targetElement;
            }
        }
    }
}