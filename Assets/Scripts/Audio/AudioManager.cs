using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace NeuralBattalion.Audio
{
    /// <summary>
    /// Singleton managing all audio playback.
    /// Responsibilities:
    /// - Play sound effects
    /// - Play background music
    /// - Manage audio volume
    /// - Audio pooling for performance
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private int sfxPoolSize = 10;

        [Header("Sound Library")]
        [SerializeField] private SoundLibrary soundLibrary;

        [Header("Settings")]
        [SerializeField] private float defaultMusicVolume = 0.7f;
        [SerializeField] private float defaultSFXVolume = 1f;
        [SerializeField] private float musicFadeDuration = 1f;

        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int currentSFXIndex = 0;
        private Coroutine musicFadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioPool();
            LoadVolumeSettings();
        }

        private void InitializeAudioPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject go = new GameObject($"SFXSource_{i}");
                go.transform.SetParent(transform);
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxPool.Add(source);
            }
        }

        private void LoadVolumeSettings()
        {
            float musicVolume = PlayerPrefs.GetFloat("NeuralBattalion_MusicVolume", defaultMusicVolume);
            float sfxVolume = PlayerPrefs.GetFloat("NeuralBattalion_SFXVolume", defaultSFXVolume);

            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
        }

        #region SFX Playback

        /// <summary>
        /// Play a sound effect by name.
        /// </summary>
        public void PlaySFX(string soundName)
        {
            AudioClip clip = soundLibrary?.GetClip(soundName);
            if (clip != null)
            {
                PlaySFX(clip);
            }
        }

        /// <summary>
        /// Play a sound effect clip.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetNextSFXSource();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }

        /// <summary>
        /// Play a sound effect at a specific position.
        /// </summary>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        /// <summary>
        /// Play a sound effect at position by name.
        /// </summary>
        public void PlaySFXAtPosition(string soundName, Vector3 position, float volume = 1f)
        {
            AudioClip clip = soundLibrary?.GetClip(soundName);
            if (clip != null)
            {
                PlaySFXAtPosition(clip, position, volume);
            }
        }

        private AudioSource GetNextSFXSource()
        {
            AudioSource source = sfxPool[currentSFXIndex];
            currentSFXIndex = (currentSFXIndex + 1) % sfxPool.Count;
            return source;
        }

        #endregion

        #region Music Playback

        /// <summary>
        /// Play background music.
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true, bool fade = true)
        {
            if (musicSource == null) return;

            if (fade && musicSource.isPlaying)
            {
                CrossfadeMusic(clip, loop);
            }
            else
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.Play();
            }
        }

        /// <summary>
        /// Play music by name.
        /// </summary>
        public void PlayMusic(string musicName, bool loop = true, bool fade = true)
        {
            AudioClip clip = soundLibrary?.GetMusicClip(musicName);
            if (clip != null)
            {
                PlayMusic(clip, loop, fade);
            }
        }

        /// <summary>
        /// Stop the current music.
        /// </summary>
        public void StopMusic(bool fade = true)
        {
            if (musicSource == null) return;

            if (fade)
            {
                StartCoroutine(FadeMusicOut());
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Pause the current music.
        /// </summary>
        public void PauseMusic()
        {
            musicSource?.Pause();
        }

        /// <summary>
        /// Resume paused music.
        /// </summary>
        public void ResumeMusic()
        {
            musicSource?.UnPause();
        }

        private void CrossfadeMusic(AudioClip newClip, bool loop)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }

            musicFadeCoroutine = StartCoroutine(CrossfadeMusicCoroutine(newClip, loop));
        }

        private System.Collections.IEnumerator CrossfadeMusicCoroutine(AudioClip newClip, bool loop)
        {
            float startVolume = musicSource.volume;

            // Fade out
            float timer = 0f;
            while (timer < musicFadeDuration / 2)
            {
                timer += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / (musicFadeDuration / 2));
                yield return null;
            }

            // Switch clip
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            // Fade in
            timer = 0f;
            while (timer < musicFadeDuration / 2)
            {
                timer += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, startVolume, timer / (musicFadeDuration / 2));
                yield return null;
            }

            musicSource.volume = startVolume;
        }

        private System.Collections.IEnumerator FadeMusicOut()
        {
            float startVolume = musicSource.volume;
            float timer = 0f;

            while (timer < musicFadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// Set master volume.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(masterVolumeParam, LinearToDecibel(volume));
            }
        }

        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(musicVolumeParam, LinearToDecibel(volume));
            }

            PlayerPrefs.SetFloat("NeuralBattalion_MusicVolume", volume);
        }

        /// <summary>
        /// Set SFX volume.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(sfxVolumeParam, LinearToDecibel(volume));
            }

            PlayerPrefs.SetFloat("NeuralBattalion_SFXVolume", volume);
        }

        private float LinearToDecibel(float linear)
        {
            return linear > 0.0001f ? 20f * Mathf.Log10(linear) : -80f;
        }

        #endregion
    }
}
