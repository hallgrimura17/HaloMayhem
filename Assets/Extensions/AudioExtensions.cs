using Assets.Resources;
using UnityEngine;

namespace Assets.Extensions
{
    public static class AudioExtensions
    {
        public static void DrawSpectrumData(this AudioSource audioSource)
        {
            float[] spectrum = new float[256];

            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

            // Loop through the populated array
            // Start the loop from 1 and to 1 less than the length, so the loop can draw lines between adjacent bins. 

            for (int i = 1; i < spectrum.Length - 1; i++)
            {
                Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
                Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
            }
        }

        //works even while muted
        public static float GetAudioClipLoudness(this AudioSource audioSource, int sampleOffset = 0, int sampleWindow = 64)
        {
            float[] waveData = new float[sampleWindow];
            float levelMax = 0;

            audioSource.clip.GetData(waveData, sampleOffset);

            foreach (float sample in waveData)
            {
                float wavePeak = Mathf.Abs(sample);
                if (levelMax < wavePeak)
                {
                    levelMax = wavePeak;
                }
            }
            return levelMax;
        }

        public static float GetAudioSourceLoudness(this AudioSource audioSource, int sampleWindow = 64)
        {
            float[] waveData = new float[sampleWindow];
            float levelMax = 0;

            audioSource.GetOutputData(waveData, 0);

            foreach (float sample in waveData)
            {
                float wavePeak = Mathf.Abs(sample);
                if (levelMax < wavePeak)
                {
                    levelMax = wavePeak;
                }
            }

            return levelMax;
        }

        public static void PlaySound(this AudioSource audioSource, string clipName, bool OneShot = false)
        {
            SoundManager.instance.PlaySoundClip(clipName, audioSource);
        }
    }
}