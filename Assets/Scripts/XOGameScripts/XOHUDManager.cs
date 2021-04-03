using System.Collections;
using System.Collections.Generic;
using TicTacToe;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.XO
{
    public class XOHUDManager : MonoBehaviour
    {
        #region Data
        public static XOHUDManager Instance { get; private set; }

        // Colors for Player 1, Player 2, and when there's a Draw
        public Color P1Color = new Color(219f / 255f, 172f / 255f, 186f / 255f);
        public Color P2Color = new Color(172f / 255f, 210f / 255f, 219f / 255f);
        public Color DrawColor = new Color(1, 1, 1);

        // UI Elements
        public Text PlayerTurnTextHUD;
        public Image PlayerTurnImageHUD;
        public Text TimeLimitLabel;
        public Text EndingLabel;
        public Text StartingLabel;
        public Button HintButton;
        public Button UndoButton;

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
        /// Initialize and reset the HUD.
        /// </summary>
        public void InitializeHUD()
        {
            // The Hint/Undo buttons are deactivated in every mode except Vs AI.
            switch (MatchManager.Instance.MatchType)
            {
                case XOMatchType.VsAI:
                    HintButton.gameObject.SetActive(true);
                    UndoButton.gameObject.SetActive(true);
                    break;
                case XOMatchType.LocalMultiplayer:
                    HintButton.gameObject.SetActive(false);
                    UndoButton.gameObject.SetActive(false);
                    break;
                case XOMatchType.CpuMatch:
                    HintButton.gameObject.SetActive(false);
                    UndoButton.gameObject.SetActive(false);
                    break;
            }

            // Set the "active player" UI element to the current player
            SetActivePlayerHUD(MatchManager.Instance.CurrentPlayer);

            // Resets all animation on the HUD.
            _animator.SetTrigger("Reset");
        }

        /// <summary>
        /// Displays the intro cutscene animation.
        /// </summary>
        /// <param name="currentPlayer">The player who gets to start the match.</param>
        public void DisplayIntro(int currentPlayer)
        {
            // Assign the label's color, depending on which player starts first.
            switch (currentPlayer)
            {
                case 1:
                    StartingLabel.color = P1Color;
                    break;
                case 2:
                    StartingLabel.color = P2Color;
                    break;
            }

            // Display text and animation
            StartingLabel.text = "Player " + currentPlayer + " Start!";
            _animator.SetTrigger("P" + currentPlayer + "Start");
        }

        /// <summary>
        /// Displays the ending cutscene animation.
        /// </summary>
        /// <param name="endingType">The type of ending reached.</param>
        public void DisplayMatchEndingText(EndingType endingType)
        {
            switch (endingType)
            {
                case EndingType.Draw:
                    EndingLabel.text = "DRAW!";
                    EndingLabel.color = DrawColor;
                    _animator.SetTrigger("Draw");
                    break;

                case EndingType.Player1Win:
                    EndingLabel.color = P1Color;
                    if (MatchManager.Instance.MatchType == XOMatchType.VsAI) // When playing Vs AI, just display "Player Wins" because there is only one real player.
                    {
                        EndingLabel.text = "PLAYER WINS!";
                    }
                    else
                    {
                        EndingLabel.text = "PLAYER 1 WINS!";
                    }
                    _animator.SetTrigger("P1Win");
                    break;

                case EndingType.Player2Win:
                    EndingLabel.color = P2Color;
                    if (MatchManager.Instance.MatchType == XOMatchType.VsAI) // When playing Vs AI, just display "You Lose..." because the only real player is the one who lost.
                    {
                        EndingLabel.text = "YOU LOSE...";
                    }
                    else
                    {
                        EndingLabel.text = "PLAYER 2 WINS!";
                    }
                    _animator.SetTrigger("P2Win");
                    break;
            }
        }

        /// <summary>
        /// Updates the Active Player HUD on screen with the new player.
        /// </summary>
        /// <param name="playerIndex">The player to display on screen.</param>
        public void SetActivePlayerHUD(int playerIndex)
        {
            PlayerTurnTextHUD.text = "Player " + playerIndex + " Turn";
            switch (playerIndex)
            {
                case 1:
                    PlayerTurnImageHUD.sprite = MatchManager.Instance.Player1Sprite;
                    break;
                case 2:
                    PlayerTurnImageHUD.sprite = MatchManager.Instance.Player2Sprite;
                    break;
            }
        }

        /// <summary>
        /// Updates the time limit display on screen
        /// </summary>
        /// <param name="newTime"></param>
        public void UpdateTimeLimit(float newTime)
        {
            if (newTime >= 1)
            {
                TimeLimitLabel.color = Color.white;
                TimeLimitLabel.text = Mathf.Ceil(newTime).ToString();
            }
            else if (newTime > 0) // Display decimal points when below 1 second and above 0 seconds.
            {
                TimeLimitLabel.color = Color.red;
                TimeLimitLabel.text = "0." + Mathf.Floor(newTime * 100);
            }
            else
            {
                TimeLimitLabel.color = Color.red;
                TimeLimitLabel.text = "0";
            }
        }

        /// <summary>
        /// Initialize component variables.
        /// </summary>
        private void Initialize()
        {
            _animator = GetComponent<Animator>();
        }
        #endregion
    }
}