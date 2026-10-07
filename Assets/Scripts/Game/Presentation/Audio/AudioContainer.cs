using System;
using FMODUnity;
using UnityEngine;

namespace Game.Presentation.Audio
{
    [CreateAssetMenu(fileName = "Audio Container", menuName = "Audio/Audio Container")]
    public class AudioContainer : ScriptableObject
    {
        [SerializeField] private EventReference _drawCard;
        [SerializeField] private EventReference _onClick;
        [SerializeField] private EventReference _pop;
        [SerializeField] private EventReference _putDown;
        [SerializeField] private EventReference _toHand;
        [SerializeField] private EventReference _yes;
        [SerializeField] private EventReference _no;

        public EventReference Get(Sound sound)
        {
            return sound switch
            {
                Sound.DrawCard => _drawCard,
                Sound.Click    => _onClick,
                Sound.Pop      => _pop,
                Sound.PutDown  => _putDown,
                Sound.ToHand   => _toHand,
                Sound.Yes      => _yes,
                Sound.No       => _no,
                _ => throw new ArgumentOutOfRangeException(nameof(sound), sound, null)
            };
        }
    }
}