using FMODUnity;
using UnityEngine;

namespace Game.Presentation.UI.Audio
{
    public enum UISound
    {
        MenuTransition,
        MenuTransitionBack,
        ButtonClick,
        BackClick,
        RequestSent,
        RequestAccepted
    }

    [CreateAssetMenu(fileName = "UIAudioConfig", menuName = "Audio/UI Audio Config")]
    public class UIAudioConfig : ScriptableObject
    {
        [field: SerializeField] public EventReference MenuTransition { get; private set; }
        [field: SerializeField] public EventReference MenuTransitionBack { get; private set; }
        [field: SerializeField] public EventReference ButtonClick { get; private set; }
        [field: SerializeField] public EventReference BackClick { get; private set; }
        [field: SerializeField] public EventReference RequestSent { get; private set; }
        [field: SerializeField] public EventReference RequestAccepted { get; private set; }

        public EventReference Get(UISound sound) => sound switch
        {
            UISound.MenuTransition => MenuTransition,
            UISound.MenuTransitionBack => MenuTransitionBack,
            UISound.ButtonClick => ButtonClick,
            UISound.BackClick => BackClick,
            UISound.RequestSent => RequestSent,
            UISound.RequestAccepted => RequestAccepted,
            _ => default
        };
    }
}