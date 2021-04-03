using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToe;
using TicTacToe.XO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TicTacToe.Tests
{
    public class HintTests
    {
        public float DelayInSeconds = 0;

        // All possible win conditions
        private int[,] _winConditions = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };

        // Spaces for the "loser" to play, shouldn't overlap with the win conditions, and should not lead to a win condition
        private int[,] _loseSetupConditions = { { 3, 6, 7 }, { 0, 6, 1 }, { 0, 3, 1 }, { 2, 1, 5 }, { 0, 2, 8 }, { 0, 1, 7 }, { 1, 3, 2 }, { 0, 1, 3 } };

        /// <summary>
        /// Load up the main game scene and set it to local multiplayer so we can decide where each X and O goes to.
        /// </summary>
        /// <returns></returns>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("MainGame");
            yield return null;
            MatchManager.Instance.MatchType = XOMatchType.LocalMultiplayer;
            MatchManager.Instance.StartMatch(false, true);
        }

        /// <summary>
        /// Checks if any hint is being displayed on an empty grid.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator DisplayHintWithEmptyGrid()
        {
            int hintedNodeIndex = MatchManager.Instance.DisplayHint();
            Assert.AreNotEqual(-1, hintedNodeIndex, "No node was hinted.");
            yield return new WaitForSeconds(0.1f);
            HintAnimationChecks(hintedNodeIndex);
            yield return new WaitForSeconds(DelayInSeconds);
        }

        /// <summary>
        /// Goes over every board configuration where the player is going to win, and checks if the hint being displayed will win the game.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator DisplayHintWithWinCondition()
        {
            XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;

            for (int conditionIndex = 0; conditionIndex < _winConditions.GetLength(0); conditionIndex++)
            {
                for (int moveIndex = 0; moveIndex < 2; moveIndex++)
                {
                    MatchManager.Instance.GridNodeSelected(gridNodes[_winConditions[conditionIndex, moveIndex]]);
                    MatchManager.Instance.GridNodeSelected(gridNodes[_loseSetupConditions[conditionIndex, moveIndex]]);
                }

                int hintedNodeIndex = MatchManager.Instance.DisplayHint();
                Assert.AreNotEqual(-1, hintedNodeIndex, "No node was hinted.");
                Assert.AreEqual(_winConditions[conditionIndex,2], hintedNodeIndex, "A node was hinted, but it was the wrong one. Node hinted was: " + hintedNodeIndex);

                yield return new WaitForSeconds(0.1f);
                HintAnimationChecks(hintedNodeIndex);
                yield return new WaitForSeconds(DelayInSeconds);

                MatchManager.Instance.StartMatch(false, true);
            }
        }

        /// <summary>
        /// Goes over every board configuration where the player is going to lose, and checks if the hint being displayed will block the "winning" move.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator DisplayHintWithLoseCondition()
        {
            XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;

            for (int conditionIndex = 0; conditionIndex < _winConditions.GetLength(0); conditionIndex++)
            {
                for (int moveIndex = 0; moveIndex < 2; moveIndex++)
                {
                    MatchManager.Instance.GridNodeSelected(gridNodes[_loseSetupConditions[conditionIndex, moveIndex]]);
                    MatchManager.Instance.GridNodeSelected(gridNodes[_winConditions[conditionIndex, moveIndex]]);
                }

                int hintedNodeIndex = MatchManager.Instance.DisplayHint();
                Assert.AreNotEqual(-1, hintedNodeIndex, "No node was hinted.");
                Assert.AreEqual(_winConditions[conditionIndex, 2], hintedNodeIndex, "A node was hinted, but it was the wrong one. Node hinted was: " + hintedNodeIndex);

                yield return new WaitForSeconds(0.1f);
                HintAnimationChecks(hintedNodeIndex);
                yield return new WaitForSeconds(DelayInSeconds);

                MatchManager.Instance.StartMatch(false, true);
            }
        }

        /// <summary>
        /// Checks to see if a hint is displayed in a random board scenario with no clear winner.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator DisplayHintWithoutAnyCondition()
        {
            /* Creating a random grid scenario with no clear winner, on X's turn
             * X O _
             * X _ O
             * O X _
             */

            XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;
            MatchManager.Instance.GridNodeSelected(gridNodes[0]);
            MatchManager.Instance.GridNodeSelected(gridNodes[6]);
            MatchManager.Instance.GridNodeSelected(gridNodes[3]);
            MatchManager.Instance.GridNodeSelected(gridNodes[5]);
            MatchManager.Instance.GridNodeSelected(gridNodes[7]);
            MatchManager.Instance.GridNodeSelected(gridNodes[1]);

            int hintedNodeIndex = MatchManager.Instance.DisplayHint();
            Assert.AreNotEqual(-1, hintedNodeIndex, "No node was hinted.");
            yield return new WaitForSeconds(0.1f);
            HintAnimationChecks(hintedNodeIndex);
            yield return new WaitForSeconds(DelayInSeconds);
        }

        /// <summary>
        /// Runs multiple Assert functions to check whether the appropriate hint animation is playing.
        /// </summary>
        /// <param name="hintedNodeIndex">The index of the node we want to check is being animated or not.</param>
        private void HintAnimationChecks(int hintedNodeIndex)
        {
            XOGridNode hintedNode = MatchManager.Instance.GridNodes[hintedNodeIndex];
            Animator nodeAnimator = hintedNode.GetComponent<Animator>();
            AnimatorClipInfo[] animationClips = nodeAnimator.GetCurrentAnimatorClipInfo(0);

            Assert.AreNotEqual(animationClips.Length, 0, "Node was hinted but no animation clips were found.");
            Assert.AreEqual("HintFlash", animationClips[0].clip.name, "Note was hinted and animation is playing, but not the correct animation.");
        }
    }
}
