using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace TicTacToe
{
    public class AudioManager : MonoBehaviour
    {
        #region Data
        public static AudioManager Instance { get; private set; }
        public float MusicVolume { get; private set; }
        public float SoundVolume { get; private set; }

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            // Defining the singleton instance
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;

                Initialize();
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Sets the music volume for the game.
        /// </summary>
        /// <param name="newVolume">The new music volume value. Anywhere between 0.0 to 1.0.</param>
        public void SetMusicVolume(float newVolume)
        {
            if (_bgmSource != null)
            {
                MusicVolume = newVolume;
                _bgmSource.outputAudioMixerGroup.audioMixer.SetFloat("BGMVolume", ConvertToDecibels(newVolume));
            }
        }

        /// <summary>
        /// Sets the sound volume for the game.
        /// </summary>
        /// <param name="newVolume">The new sound volume value. Anywhere between 0.0 to 1.0.</param>
        public void SetSoundVolume(float newVolume)
        {
            if (_sfxSource != null)
            {
                SoundVolume = newVolume;
                _sfxSource.outputAudioMixerGroup.audioMixer.SetFloat("SFXVolume", ConvertToDecibels(newVolume));
            }
        }

        /// <summary>
        /// Plays an audio clip on the main sound effect audio source.
        /// </summary>
        /// <param name="clipToPlay">The audio clip to play.</param>
        public void PlaySoundClip(AudioClip clipToPlay)
        {
            if (_sfxSource != null)
            {
                _sfxSource.PlayOneShot(clipToPlay);
            }
        }

        /// <summary>
        /// Initializes variables and components.
        /// </summary>
        private void Initialize()
        {
            // Default volume is 1 (100% volume)
            MusicVolume = 1;
            SoundVolume = 1;

            // The structure of this object must contain a child gameobject "BGM" with an AudioSource, as well as a child gameobject "SFX" with an AudioSource
            Transform bgmChild = transform.Find("BGM");
            if (bgmChild != null)
            {
                _bgmSource = bgmChild.GetComponent<AudioSource>();
            }

            Transform sfxChild = transform.Find("SFX");
            if (sfxChild != null)
            {
                _sfxSource = sfxChild.GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Takes a float value between 0.0 and 1.0, and converts it into an appropriate decibel value for sound.
        /// </summary>
        /// <param name="oldValue">Value to convert to decibels. Between 0.0 and 1.0.</param>
        /// <returns>The converted decibel value.</returns>
        private float ConvertToDecibels(float oldValue)
        {
            return oldValue == 0 ? -80 : 20f * Mathf.Log10(oldValue);
        }
        #endregion
    }
}