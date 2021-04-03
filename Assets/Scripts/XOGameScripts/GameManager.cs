using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TicTacToe.XO;

namespace TicTacToe
{
    public class GameManager : MonoBehaviour
    {
        #region Data
        public static GameManager Instance { get; private set; }

        public GraphicBundlePack CurrentGraphicPack { get; private set; }

        private bool _isTransitioning; // Flag to make sure the game doesn't start a new scene transition during an existing transition
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
                DontDestroyOnLoad(gameObject); // The GameManager object and all it's children are singletons, and are persistent throughout the whole game.
            }
        }

        private void Start()
        {
            InitializeScene(); // Load up the proper events for the current scene
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Calls the MainGame scene with a fading transition effect, as well as setting up the MatchManager
        /// with the proper MatchType and AILevel.
        /// </summary>
        /// <param name="matchType">The type of match to take place.</param>
        /// <param name="difficulty">The difficulty of the AI.</param>
        public void StartGame(XOMatchType matchType, AILevel difficulty)
        {
            StartCoroutine(GameStartEnumerator(matchType, difficulty));
        }

        /// <summary>
        /// Loads a new target scene. Can use special transition effects between scenes.
        /// </summary>
        /// <param name="targetScene">The scene to load into.</param>
        /// <param name="transitionType">The type of transition into the target scene.</param>
        public void GotoScene(string targetScene, TransitionType transitionType)
        {
            switch (transitionType)
            {
                case TransitionType.Fade:
                    StartCoroutine("FadeTransition", targetScene);
                    break;
                case TransitionType.Instant:
                    SceneManager.LoadScene(targetScene);
                    break;
            }
        }

        /// <summary>
        /// Set the current skin pack of the game.
        /// </summary>
        /// <param name="newGraphicPack">The new graphic skin pack to apply.</param>
        public void ApplySkin(GraphicBundlePack newGraphicPack)
        {
            CurrentGraphicPack = newGraphicPack;
        }

        /// <summary>
        /// Calls the appropriate events when GameManager is created dependant on the scene.
        /// Mainly used for debug, as the regular player will always enter the Landing scene first.
        /// </summary>
        private void InitializeScene()
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "Landing":
                    GotoScene("MainMenu", TransitionType.Fade);
                    break;
                case "MainGame":
                    if (MatchManager.Instance != null)
                    {
                        MatchManager.Instance.StartMatch(false, true);
                    }
                    break;
            }
        }

        /// <summary>
        /// Coroutine for loading a new scene with a fade effect.
        /// </summary>
        /// <param name="targetScene">The scene to load into.</param>
        /// <returns></returns>
        private IEnumerator FadeTransition(string targetScene)
        {
            if (!_isTransitioning)
            {
                _isTransitioning = true;

                TransitionManager.Instance.SetFade(true); // Call the transition manager to fade *out* the screen
                yield return new WaitForSecondsRealtime(0.5f); // Delay for the fade to complete itself

                SceneManager.LoadScene(targetScene);

                TransitionManager.Instance.SetFade(false); // Call the transition manager to fade *in* the screen
                yield return null;

                _isTransitioning = false;
            }
        }

        /// <summary>
        /// Coroutine for starting a new game.
        /// </summary>
        /// <param name="matchType">The type of match to setup.</param>
        /// <param name="difficulty">The difficulty level of the AI.</param>
        /// <returns></returns>
        private IEnumerator GameStartEnumerator(XOMatchType matchType, AILevel difficulty)
        {
            // Fade out screen
            yield return FadeTransition("MainGame");


            if (MatchManager.Instance == null) // In case of error, load up the landing scene instead
            {
                GotoScene("Landing", TransitionType.Instant);
            }
            else
            {
                // Initialize MatchManager values and start the match
                MatchManager.Instance.SetMatchTypeAndDifficulty(matchType, difficulty);
                MatchManager.Instance.InitializeAssets();
                MatchManager.Instance.StartMatch(true, false);
            }
        }
        #endregion
    }
}