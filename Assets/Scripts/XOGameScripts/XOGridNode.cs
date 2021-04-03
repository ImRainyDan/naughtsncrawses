using System.Collections;
using System.Collections.Generic;
using TicTacToe;
using UnityEngine;

namespace TicTacToe.XO
{
    public class XOGridNode : MonoBehaviour
    {
        #region Data
        public int NodeIndex; // The index of the node on the screen. ('0' starts at the top-left, and then moves left-to-right across each row from top-to-bottom.)
        public int NodeState { get; private set; } // The current state of the node. ('0': Empty, '1': Player 1, '2': Player 2)

        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            Initialize(); // Get component variables and reset the node.
        }

        private void OnMouseDown()
        {
            NodeSelected(); // Calls the node selection function when clicked by the mouse.
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Assigns the nodes value and updates it's sprite + animation, while playing the correct sound effect.
        /// </summary>
        /// <param name="newNodeState">The node state to assign. 1: Player 1, 2: Player 2.</param>
        public void SetNodeValue(int newNodeState)
        {
            AudioClip selectedAudioClip;

            NodeState = newNodeState;
            switch (newNodeState)
            {
                case 0:
                    _spriteRenderer.sprite = null;
                    break;
                case 1:
                    _spriteRenderer.sprite = MatchManager.Instance.Player1Sprite;
                    break;
                case 2:
                    _spriteRenderer.sprite = MatchManager.Instance.Player2Sprite;
                    break;
            }

            if (newNodeState != 0) // Apply animation and sound if the node has gained a non-empty value.
            {
                _animator.SetTrigger("Selected");

                // Cross always starts first, so if the CurrentTurn variable is an odd number, we play the cross sound effect, otherwise we play the circle sound effect.
                selectedAudioClip = MatchManager.Instance.CurrentTurn % 2 == 0 ? MatchManager.Instance.CircleSound : MatchManager.Instance.CrossSound;
                AudioManager.Instance.PlaySoundClip(selectedAudioClip);
            }
        }

        /// <summary>
        /// Displays hint animation on the current node.
        /// </summary>
        /// <param name="currentPlayerIndex">The current acting player.</param>
        public void FlashHintGraphic(int currentPlayerIndex)
        {
            // The hint animation can only play if the current node is empty.
            if (NodeState == 0)
            {
                // Assign the appropriate sprite for the blinking hint, dependant on the current player.
                switch (currentPlayerIndex) 
                {
                    case 1:
                        _spriteRenderer.sprite = MatchManager.Instance.Player1Sprite;
                        break;
                    case 2:
                        _spriteRenderer.sprite = MatchManager.Instance.Player2Sprite;
                        break;
                }

                _animator.SetTrigger("Hint");
            }
        }

        /// <summary>
        /// Reset the nodes value and animation/sprite.
        /// </summary>
        public void ResetNode()
        {
            SetNodeValue(0);
            _animator.SetTrigger("Reset");
        }

        /// <summary>
        /// Initialize component variables. Resets nodes value and animation.
        /// </summary>
        private void Initialize()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();

            ResetNode();
        }

        /// <summary>
        /// Calls the GridNodeSelected function on the MatchManager.
        /// </summary>
        private void NodeSelected()
        {
            // You can only select a node if it's empty.
            if (NodeState == 0)
            {
                switch (MatchManager.Instance.MatchType)
                {
                    case XOMatchType.LocalMultiplayer:
                        MatchManager.Instance.GridNodeSelected(this); // In local multiplayer, you can always click on nodes to select them.
                        break;
                    case XOMatchType.VsAI:
                        if (MatchManager.Instance.CurrentPlayer == 1) // In vs AI, you can only click on the node if it's your turn.
                        {
                            MatchManager.Instance.GridNodeSelected(this);
                        }
                        break;
                }
            }
        }
        #endregion
    }
}