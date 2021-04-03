using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe
{
    public class TransitionManager : MonoBehaviour
    {
        #region Data
        public static TransitionManager Instance { get; private set; }

        private Animator _animator;
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

                Initialize(); // Initialize component variables
            }
        }
        #endregion

        #region Custom Methods

        /// <summary>
        /// Triggers the fade animation on the screen.
        /// </summary>
        /// <param name="fadeOut">A value of 'true' will cause the screen to fade out. A value of 'false' will cause the screen to fade in.</param>
        public void SetFade(bool fadeOut)
        {
            if (fadeOut)
            {
                _animator.SetTrigger("FadeOut");
            }
            else
            {
                _animator.SetTrigger("FadeIn");
            }
        }

        /// <summary>
        ///  Initializes component variables.
        /// </summary>
        private void Initialize()
        {
            _animator = GetComponent<Animator>();
        }
        #endregion
    }
}