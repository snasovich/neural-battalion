using UnityEngine;
using System;
using System.Collections.Generic;

namespace NeuralBattalion.Audio
{
    /// <summary>
    /// ScriptableObject containing all sound effects and music references.
    /// Provides easy access to audio clips by name.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Neural Battalion/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        [Serializable]
        public class SoundEntry
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)] public float defaultVolume = 1f;
            [Range(0.5f, 2f)] public float defaultPitch = 1f;
            public bool randomizePitch = false;
            [Range(0f, 0.5f)] public float pitchVariation = 0.1f;
        }

        [Header("Sound Effects")]
        [SerializeField] private SoundEntry[] soundEffects;

        [Header("Music Tracks")]
        [SerializeField] private SoundEntry[] musicTracks;

        // Cache for faster lookup
        private Dictionary<string, SoundEntry> sfxCache;
        private Dictionary<string, SoundEntry> musicCache;

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            sfxCache = new Dictionary<string, SoundEntry>();
            musicCache = new Dictionary<string, SoundEntry>();

            if (soundEffects != null)
            {
                foreach (var entry in soundEffects)
                {
                    if (!string.IsNullOrEmpty(entry.name) && !sfxCache.ContainsKey(entry.name))
                    {
                        sfxCache[entry.name] = entry;
                    }
                }
            }

            if (musicTracks != null)
            {
                foreach (var entry in musicTracks)
                {
                    if (!string.IsNullOrEmpty(entry.name) && !musicCache.ContainsKey(entry.name))
                    {
                        musicCache[entry.name] = entry;
                    }
                }
            }
        }

        /// <summary>
        /// Get a sound effect clip by name.
        /// </summary>
        public AudioClip GetClip(string name)
        {
            if (sfxCache == null) BuildCache();

            if (sfxCache.TryGetValue(name, out SoundEntry entry))
            {
                return entry.clip;
            }

            Debug.LogWarning($"[SoundLibrary] Sound effect '{name}' not found");
            return null;
        }

        /// <summary>
        /// Get a music clip by name.
        /// </summary>
        public AudioClip GetMusicClip(string name)
        {
            if (musicCache == null) BuildCache();

            if (musicCache.TryGetValue(name, out SoundEntry entry))
            {
                return entry.clip;
            }

            Debug.LogWarning($"[SoundLibrary] Music track '{name}' not found");
            return null;
        }

        /// <summary>
        /// Get full sound entry for more control.
        /// </summary>
        public SoundEntry GetSoundEntry(string name)
        {
            if (sfxCache == null) BuildCache();
            sfxCache.TryGetValue(name, out SoundEntry entry);
            return entry;
        }

        /// <summary>
        /// Get pitch with optional randomization.
        /// </summary>
        public float GetPitch(string name)
        {
            var entry = GetSoundEntry(name);
            if (entry == null) return 1f;

            if (entry.randomizePitch)
            {
                return entry.defaultPitch + UnityEngine.Random.Range(-entry.pitchVariation, entry.pitchVariation);
            }

            return entry.defaultPitch;
        }

        /// <summary>
        /// Check if a sound exists.
        /// </summary>
        public bool HasSound(string name)
        {
            if (sfxCache == null) BuildCache();
            return sfxCache.ContainsKey(name);
        }

        /// <summary>
        /// Check if a music track exists.
        /// </summary>
        public bool HasMusic(string name)
        {
            if (musicCache == null) BuildCache();
            return musicCache.ContainsKey(name);
        }

        /// <summary>
        /// Get all sound effect names.
        /// </summary>
        public string[] GetAllSoundNames()
        {
            if (sfxCache == null) BuildCache();
            string[] names = new string[sfxCache.Count];
            sfxCache.Keys.CopyTo(names, 0);
            return names;
        }

        /// <summary>
        /// Get all music track names.
        /// </summary>
        public string[] GetAllMusicNames()
        {
            if (musicCache == null) BuildCache();
            string[] names = new string[musicCache.Count];
            musicCache.Keys.CopyTo(names, 0);
            return names;
        }
    }
}
