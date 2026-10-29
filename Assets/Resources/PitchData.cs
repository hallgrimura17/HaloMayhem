using UnityEngine;

namespace Assets.Resources
{
    [CreateAssetMenu(fileName = "PitchData", menuName = "PitchData")]
    public class PitchData : ScriptableObject
    {
        public float Pitch = 1f, PitchDeviation = 0.1f;
    }
}

