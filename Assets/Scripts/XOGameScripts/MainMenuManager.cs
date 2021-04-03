using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TicTacToe;
using TicTacToe.XO;

namespace TicTacToe.XO
{
    public class MainMenuManager : MonoBehaviour
    {
        #region Data
        public static MainMenuManager Instance { get; private set; }
        public Toggle FullscreenToggle;
        public InputField ReskinField;
        public Slider BGMSlider;
        public Slider SFXSlider;
        public AudioClip CursorSound;

        private Animator _animator;
        private int _currentPage = 0;
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

                Initialize(); // Initialize private component variables
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Press event for the Player vs Player button
        /// </summary>
        public void PlayerVsPlayerPress()
        {
            AudioManager.Instance.PlaySoundClip(CursorSound);
            GameManager.Instance.StartGame(XOMatchType.LocalMultiplayer, AILevel.Easy);
        }

        /// <summary>
        /// Press event for the Computer vs Computer button
        /// </summary>
        public void CpuVsCpuPress()
        {
            AudioManager.Instance.PlaySoundClip(CursorSound);
            GameManager.Instance.StartGame(XOMatchType.CpuMatch, AILevel.Medium);
        }

        /// <summary>
        /// Press event for the difficulty selection button
        /// </summary>
        /// <param name="difficulty">The selected difficulty level. (1 = Easy, 2 = Medium, 3 = Hard)</param>
        public void SelectDifficulty(int difficulty)
        {
            AudioManager.Instance.PlaySoundClip(CursorSound);
            GameManager.Instance.StartGame(XOMatchType.VsAI, (AILevel)difficulty);
        }

        /// <summary>
        /// Moves the menu screen to the selected page.
        /// </summary>
        /// <param name="page">The menu page to move to.</param>
        public void GoToPage(int page)
        {
            if (_currentPage != page)
            {
                _currentPage = page;

                AudioManager.Instance.PlaySoundClip(CursorSound);
                _animator.SetInteger("MenuState", page);

                // Make sure the UI elements on the option screen are synced to their real values.
                FullscreenToggle.isOn = Screen.fullScreen;
                BGMSlider.value = AudioManager.Instance.MusicVolume;
                SFXSlider.value = AudioManager.Instance.SoundVolume;
            }
        }

        /// <summary>
        /// Press event for the Toggle Fullscreen button
        /// </summary>
        public void ToggleFullscreen()
        {
            AudioManager.Instance.PlaySoundClip(CursorSound);
            Screen.fullScreen = FullscreenToggle.isOn;
        }

        /// <summary>
        /// Slider value changed event for the Music Volume slider
        /// </summary>
        /// <param name="newValue">The new volume value. (From 0.0 to 1.0)</param>
        public void SetMusicVolume(float newValue)
        {
            AudioManager.Instance.SetMusicVolume(newValue);
        }

        /// <summary>
        /// Slider value changed event for the Sound Volume slider
        /// </summary>
        /// <param name="newValue">The new volume value. (From 0.0 to 1.0)</param>
        public void SetSoundVolume(float newValue)
        {
            AudioManager.Instance.SetSoundVolume(newValue);
        }

        /// <summary>
        /// Press event for the Reskin button. Changes the current loaded graphic bundle skin.
        /// </summary>
        public void ReskinPress()
        {
            AudioManager.Instance.PlaySoundClip(CursorSound);

            // Load the selected bundle skin entered in the ReskinField, looking through the streaming assets folder.
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, ReskinField.text));
            if (bundle == null) 
            {
                // Display error if no bundle was found in the streaming assets folder.
                ReskinField.text = "Loading failed!";
            }
            else
            {
                // Unpack bundle and load assets
                Sprite crossSprite = bundle.LoadAsset<Sprite>("Cross");
                Sprite circleSprite = bundle.LoadAsset<Sprite>("Circle");
                Sprite backgroundGraphic = bundle.LoadAsset<Sprite>("Background");
                bundle.Unload(false);

                GraphicBundlePack loadedGraphicPack = new GraphicBundlePack(ReskinField.text, crossSprite, circleSprite, backgroundGraphic);

                GameManager.Instance.ApplySkin(loadedGraphicPack); // Pass new GraphicBundlePack struct to the game manager.

                ReskinField.text = "Loaded!";
            }
        }

        /// <summary>
        /// Press event for the Exit button
        /// </summary>
        public void ExitGame()
        {
            AudioManager.Instance.PlaySoundClip(CursorSound);
            Application.Quit();
        }

        /// <summary>
        /// Initializes variables and components
        /// </summary>
        private void Initialize()
        {
            _animator = GetComponent<Animator>();
        }
        #endregion
    }
}