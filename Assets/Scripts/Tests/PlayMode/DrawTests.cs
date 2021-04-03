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
    public class DrawTests
    {
        public float DelayInSeconds = 0f;

        private int[,] _drawScenarios = { { 0, 1, 2, 4, 3, 5, 7, 6, 8 }, { 0, 2, 1, 3, 5, 4, 8, 7, 6 }, { 0, 2, 1, 3, 5, 4, 6, 7, 8 } };

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
        /// This test will go try scenarios where the entire board is filled and make sure that the game recognized that scenario as a draw
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator CheckDrawConditions()
        {
            for (int drawScenarioIndex = 0; drawScenarioIndex < _drawScenarios.GetLength(0); drawScenarioIndex++)
            {
                XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;
                for (int nodeIndex = 0; nodeIndex < _drawScenarios.GetLength(1); nodeIndex++)
                {
                    MatchManager.Instance.GridNodeSelected(gridNodes[_drawScenarios[drawScenarioIndex,nodeIndex]]);
                    EndingType endType = MatchManager.Instance.CheckGameEnd(_drawScenarios[drawScenarioIndex, nodeIndex]);
                    if (nodeIndex == _drawScenarios.GetLength(1) - 1)
                    {
                        Assert.AreEqual(EndingType.Draw, endType, "All spaces are filled but it's not a draw.");
                    }
                    else
                    {
                        Assert.AreEqual(EndingType.None, endType, "The game ended before it could reach a 'draw' scenario.");
                    }
                }

                yield return new WaitForSeconds(DelayInSeconds);
                Assert.IsTrue(IsEndingAnimationPlaying(), "The ending animation is not playing when there was a draw.");
                MatchManager.Instance.StartMatch(false, true);
                yield return null;
            }
        }

        /// <summary>
        /// Checks whether or not the "Draw" animation is playing on the HUD Manager
        /// </summary>
        /// <returns>Returns true if the "Draw" animation is currently playing, false otherwise</returns>
        private bool IsEndingAnimationPlaying()
        {
            bool result = false;

            Animator hudAnimator = XOHUDManager.Instance.GetComponent<Animator>();
            AnimatorClipInfo[] animationClips = hudAnimator.GetCurrentAnimatorClipInfo(0);
            if (animationClips.Length > 0)
            {
                if (animationClips[0].clip.name == "MatchHUDDraw")
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
