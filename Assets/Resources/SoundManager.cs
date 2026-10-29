using Assets.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Resources
{
    public class SoundManager : MonoBehaviour
    {
        private Dictionary<string, AudioClip> sounds;
        private static SoundManager _instance;
        public List<SoundByte> SoundBytes = new();
        [SerializeField]
        public Dictionary<string, SoundByte> SoundClipLookup = new();
        public int AmountOfAudioSources = 20;

        public static SoundManager instance
        {
            get
            {
                if (!_instance)
                    _instance = Instantiate(UnityEngine.Resources.Load<SoundManager>(nameof(SoundManager)));
                return _instance;
            }
        }

        private AudioSource[] audioss;
        private Dictionary<EntityId, KeyValuePair<float, AudioSource>> externalAudios;
        private int audiossIndex;
        private HashSet<string> soundsThisFrame = new();

        public Transform player
        {
            get
            {
                if (_player == null)
                {
                    _player = Camera.main.transform.root;
                }

                return _player;
            }
        }

        private Transform _player;

        // Start is called before the first frame update
        void Awake()
        {
            audioss = GetComponentsInChildren<AudioSource>();
            sounds = new Dictionary<string, AudioClip>();
            SoundClipLookup = SoundBytes.ToDictionary(x => x.name);
            externalAudios = new ();
        }

#if UNITY_EDITOR
        public bool Run = false;
        public void OnValidate()
        {
            if (Run)
            {
                transform.SetAmountOfChildren(AmountOfAudioSources, true);
                Run = false;
                // Search for all prefabs within the "Assets/Prefabs" directory
                string[] guids = AssetDatabase.FindAssets("t:SoundByte glob:\"Assets/Resources/Sounds/SoundBytes/*.asset\"");

                Debug.Log($"Found {guids.Length} prefabs in Assets/Prefabs folder.");
                SoundBytes.Clear();
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var itemData = AssetDatabase.LoadAssetAtPath(assetPath, typeof(SoundByte)) as SoundByte;
                    SoundBytes.Add(itemData);
                }
            }
        }
#endif

        void FixedUpdate()
        {
            soundsThisFrame.Clear();
        }

        public SoundByte GetSoundByte(string name)
        {
            if (!SoundClipLookup.TryGetValue(name, out var soundClip))
            {
                Debug.LogError($"Couldn't find SoundClip {name}");
                return null;
            }
            return soundClip;
        }

        public void TimeChange()
        {
            try
            {
                var timeScale = Time.timeScale;
                foreach (var externalAudio in externalAudios.ToArray())
                {
                    if (externalAudio.Value.Value != null)
                    {
                        externalAudio.Value.Value.pitch = externalAudio.Value.Key * timeScale;
                        //Debug.Log(externalAudio.Value.Key + ", " + timeScale + ", " + externalAudio.Value.Value.pitch + ", " + externalAudio.Value.Value.isPlaying);
                    }
                    else
                    {
                        externalAudios.Remove(externalAudio.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void PauseSounds()
        {
            try
            {
                foreach (var externalAudio in externalAudios.Where(x => x.Value.Value.isPlaying))
                {
                    if (externalAudio.Value.Value != null)
                    {
                        externalAudio.Value.Value.Pause();
                        //Debug.Log(externalAudio.Value.Key + ", " + timeScale + ", " + externalAudio.Value.Value.pitch + ", " + externalAudio.Value.Value.isPlaying);
                    }
                    else
                    {
                        externalAudios.Remove(externalAudio.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public void UnPauseSounds()
        {
            try
            {
                foreach (var externalAudio in externalAudios.ToArray())
                {
                    if (externalAudio.Value.Value != null)
                    {
                        externalAudio.Value.Value.UnPause();
                        //Debug.Log(externalAudio.Value.Key + ", " + timeScale + ", " + externalAudio.Value.Value.pitch + ", " + externalAudio.Value.Value.isPlaying);
                    }
                    else
                    {
                        externalAudios.Remove(externalAudio.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private AudioSource GetNextAudioSource()
        {
            var index = audiossIndex % audioss.Length;
            var audioSource = audioss[index];
            audiossIndex++;
            return audioSource;
        }

        public void PlaySoundClipAtLocation(Vector3 position, string name, float pitchOverride = 0, AudioSource audios = null)
        {
            if(audios == null)
                audios = GetNextAudioSource();
            audios.transform.position = position;
            PlaySoundClip(name, audios, pitchOverride);
        }

        public void PlaySoundClip(string name, AudioSource audios = null, float pitchOverride = 0)
        {
            if (Time.timeScale > 0 && soundsThisFrame.Contains(name))
            {
                return;
            }
            if(audios == null)
            {
                audios = GetNextAudioSource();
            }

            soundsThisFrame.Add(name);
            var entityId = audios.GetEntityId();

            var soundClip = GetSoundByte(name);
            var pitch = (pitchOverride > 0 ? pitchOverride :  soundClip.PitchData.Pitch) * (1 + Random.value * soundClip.PitchData.PitchDeviation - soundClip.PitchData.PitchDeviation * 0.5f);

            if (!externalAudios.ContainsKey(entityId))
            {
                externalAudios.Add(entityId, new KeyValuePair<float, AudioSource>(pitch, audios));
            }
            else if (externalAudios[entityId].Key != pitch)
            {
                externalAudios[entityId] = new KeyValuePair<float, AudioSource>(pitch, audios);
            }

            if (audios.isPlaying && audios.priority < soundClip.Priority)
            {
                return;
            }

            audios.pitch = pitch;
            audios.priority = soundClip.Priority;
            audios.volume = soundClip.Volume;
            var clip = SoundBytes.Count == 1 ? soundClip.clips.First() : soundClip.clips.RandomElement();

            if(audios.isPlaying)
                audios.PlayOneShot(clip);
            else
            {
                audios.clip = clip;
                audios.Play();
            }
        }

        public AudioClip GetAudioClip(string name)
        {
            var soundClip = GetSoundByte(name);
            return SoundBytes.Count == 1 ? soundClip.clips.First() : soundClip.clips.RandomElement();
        }
    }
}

