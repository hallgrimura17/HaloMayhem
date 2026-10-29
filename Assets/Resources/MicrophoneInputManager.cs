using Assets.Extensions;
using UnityEngine;

[RequireComponent (typeof(AudioSource))]
public class MicrophoneInputManager : MonoBehaviour
{
    private static MicrophoneInputManager _instance;
    public static MicrophoneInputManager instance
    {
        get
        {
            if (!_instance)
                _instance = Instantiate(Resources.Load<MicrophoneInputManager>(nameof(MicrophoneInputManager)));
            return _instance;
        }
    }
    private AudioSource audioSource;
    private string deviceName;
    public int RecordingLength = 10;
    public float TimeElapsed;

    public void Update()
    {
        TimeElapsed = audioSource.time;
        //audioSource.DrawSpectrumData();
    }

    public AudioSource GetMicAudioSource => audioSource;

    void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();

        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0];
            audioSource.clip = Microphone.Start(deviceName, true, RecordingLength, AudioSettings.outputSampleRate);
            while(!(Microphone.GetPosition(null) > 0)){ }
            audioSource.Play();
            audioSource.ignoreListenerPause = true;
        }
        else
        {
            Debug.LogWarning("No microphone devices found!");
        }
    }

    public float GetPlayerLoudness()
    {
        return audioSource.GetAudioClipLoudness(audioSource.timeSamples);
    }

    void OnDisable()
    {
        Microphone.End(deviceName);
    }
}