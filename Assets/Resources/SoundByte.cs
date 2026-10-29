using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources
{
    [CreateAssetMenu(fileName = "SoundByte", menuName = "SoundByte")]
    public class SoundByte : ScriptableObject
    {
        [SerializeField]
        public List<AudioClip> clips;
        [SerializeField]
        public PitchData PitchData;
        public int Priority = 128;
        public float Volume = 1f;
    }
}

