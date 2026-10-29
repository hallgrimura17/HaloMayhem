using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources
{
    public class ParticleManager : MonoBehaviour
    {
        private static ParticleManager _instance;

        public static ParticleManager instance
        {
            get
            {
                if (!_instance)
                {
                    var particleManager = Instantiate(UnityEngine.Resources.Load<ParticleManager>(nameof(ParticleManager)));
                    var persistentManager = GameObject.Find("SingletonManager");
                    if (persistentManager)
                        particleManager.transform.SetParent(persistentManager.transform);
                    _instance = particleManager;
                }

                return _instance;
            }
        }

        [SerializeField]
        public Dictionary<string, ParticleSystem> ParticleLookup = new();
        [SerializeField]
        public List<ParticleSystem> Particles = new List<ParticleSystem>();

        public void OnValidate()
        {
            Awake();
        }

        public void Awake()
        {
            ParticleLookup.Clear();
            Particles.Clear();
            foreach (Transform child in transform)
            {
                var ps = child.GetComponent<ParticleSystem>();
                if (ps == null) continue;
                Particles.Add(ps);
                ParticleLookup.Add(child.name, ps);
                var main = ps.main;
                main.playOnAwake = false;
            }
            GetComponentsInChildren<MeshRenderer>().ToList().ForEach(x => x.gameObject.SetActive(false));
        }

        public void PlayParticleEffect(string name, Vector3 position, Quaternion rotation)
        {
            var particleSystem = MoveParticleSystem(name, position, rotation);
            if (particleSystem != null) 
                particleSystem.Play(true);
        }

        public ParticleSystem GetParticleSystem(string name)
        {
            if (!ParticleLookup.TryGetValue(name, out var particleSystem))
            {
                Debug.LogError($"Did not find particle system: {name}");
            }
            return particleSystem;
        }

        public ParticleSystem MoveParticleSystem(string name, Vector3 position, Quaternion rotation)
        {
            var particleSystem = GetParticleSystem(name);
            MoveParticleSystem(particleSystem, position, rotation);
            return particleSystem;
        }

        public void MoveParticleSystem(ParticleSystem particleSystem, Vector3 position, Quaternion rotation)
        {
            particleSystem.transform.position = position;
            particleSystem.transform.rotation = rotation;
        }

        public void EmitParticleEffect(string name, Vector3 position, Quaternion rotation, int amount)
        {
            var particleSystem = MoveParticleSystem(name, position, rotation);
            if (particleSystem != null) 
                particleSystem.Emit(amount);
        }
    }
}