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
    public class WinTests
    {
        public float DelayInSeconds = 0f;

        // All possible win conditions
        private int[,] _winConditions = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };

        // Spaces for the "loser" to play, shouldn't overlap with the win conditions, and should not lead to a win condition
        private int[,] _loseSetupConditions = { { 3, 6, 7 }, { 0, 6, 1 }, { 0, 3, 1 }, { 2, 1, 5 }, { 0, 2, 8 }, { 0, 1, 7 }, { 1, 3, 2 }, { 0, 1, 3 } };

        /// <summary>
        /// Load up the main game scene and set it to local multiplayer so we can decide where each X and O goes to
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
        /// Goes over board scenarios without any winner or lose, and checks if the game doesn't accidentally recognize them as a win scenario.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CheckConditionsWithoutWinners()
        {
            XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;

            for (int conditionIndex = 0; conditionIndex < _winConditions.GetLength(0); conditionIndex++)
            {
                for (int moveIndex = 0; moveIndex < 2; moveIndex++)
                {
                    MatchManager.Instance.GridNodeSelected(gridNodes[_winConditions[conditionIndex, moveIndex]]);
                    MatchManager.Instance.GridNodeSelected(gridNodes[_loseSetupConditions[conditionIndex, moveIndex]]);
                    EndingType endType = MatchManager.Instance.CheckGameEnd(_winConditions[conditionIndex, moveIndex]);
                    Assert.AreNotEqual(EndingType.Player1Win, endType, "Player 1 won when they shouldn't have.");
                }

                yield return new WaitForSeconds(DelayInSeconds);

                Assert.IsFalse(IsEndingAnimationPlaying(), "The ending animation is playing when no one won.");
                MatchManager.Instance.StartMatch(false, true);

                yield return null;
            }
        }

        /// <summary>
        /// Goes over all possible win conditions for player 1, and checks if the game recognizes them as a win scenario.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CheckAllWinConditions()
        {
            XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;

            for (int conditionIndex = 0; conditionIndex < _winConditions.GetLength(0); conditionIndex++)
            {
                for (int moveIndex = 0; moveIndex < 3; moveIndex++)
                {
                    MatchManager.Instance.GridNodeSelected(gridNodes[_winConditions[conditionIndex, moveIndex]]);
                    if (moveIndex <= 1)
                    {
                        MatchManager.Instance.GridNodeSelected(gridNodes[_loseSetupConditions[conditionIndex, moveIndex]]);
                    }
                    else
                    {
                        EndingType endType = MatchManager.Instance.CheckGameEnd(_winConditions[conditionIndex, moveIndex]);
                        Assert.AreEqual(EndingType.Player1Win, endType, "Player 1 didn't win when they should have.");
                    }
                }
                
                yield return new WaitForSeconds(DelayInSeconds);

                Assert.IsTrue(IsEndingAnimationPlaying(), "The ending animation is not playing when Player 1 won.");
                MatchManager.Instance.StartMatch(false, true);

                yield return null;
            }
        }

        /// <summary>
        /// Checks whether or not the "P1Win" animation is playing on the HUD Manager
        /// </summary>
        /// <returns>Returns true if the "P1Win" animation is currently playing, false otherwise</returns>
        private bool IsEndingAnimationPlaying()
        {
            bool result = false;

            Animator hudAnimator = XOHUDManager.Instance.GetComponent<Animator>();
            AnimatorClipInfo[] animationClips = hudAnimator.GetCurrentAnimatorClipInfo(0);
            if (animationClips.Length > 0)
            {
                if (animationClips[0].clip.name == "MatchHUDP1Win")
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
