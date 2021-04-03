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
    public class UndoTests
    {
        public float DelayInSeconds = 0f;

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
        /// Checks if trying to undo on an empty board results in anything abnormal.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UndoOnEmptyGrid()
        {
            List<int> movesUndone = MatchManager.Instance.UndoLastMoves();
            Assert.AreEqual(0, movesUndone.Count, "At least 1 move was undone on an empty grid.");
            Assert.AreEqual(0, GetGridFullSpacesCount(), "The grid should be empty but it has " + GetGridFullSpacesCount() + " filled spaces.");
            yield return new WaitForSeconds(DelayInSeconds);
        }

        /// <summary>
        /// Sets up a grid where 1 move was done by both the player and the AI, and undoes it. Checks if the correct nodes were undone.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UndoOneTurn()
        {
            XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;
            MatchManager.Instance.GridNodeSelected(gridNodes[0]);
            MatchManager.Instance.GridNodeSelected(gridNodes[1]);
            yield return new WaitForSeconds(DelayInSeconds);

            List<int> movesUndone = MatchManager.Instance.UndoLastMoves();
            Assert.Greater(movesUndone.Count, 0, "No moves were undone.");
            Assert.Contains(0, movesUndone, "A move that was expected to be undone has not actually be undone: 0");
            Assert.Contains(1, movesUndone, "A move that was expected to be undone has not actually be undone: 1");
            Assert.AreEqual(0, GetGridFullSpacesCount(), "The grid should be empty but it has " + GetGridFullSpacesCount() + " filled spaces.");
            yield return new WaitForSeconds(DelayInSeconds);
        }

        /// <summary>
        /// Sets up a grid scenario where 2 moves were done by both the player and the AI, and undoes them both. Checks if the correct nodes were undone.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UndoTwoTurns()
        {
            XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;
            MatchManager.Instance.GridNodeSelected(gridNodes[0]);
            MatchManager.Instance.GridNodeSelected(gridNodes[1]);
            MatchManager.Instance.GridNodeSelected(gridNodes[2]);
            MatchManager.Instance.GridNodeSelected(gridNodes[3]);
            yield return new WaitForSeconds(DelayInSeconds);

            List<int> movesUndone = MatchManager.Instance.UndoLastMoves();
            Assert.Greater(movesUndone.Count, 0, "No moves were undone.");
            Assert.Contains(2, movesUndone, "A move that was expected to be undone has not actually be undone: 2");
            Assert.Contains(3, movesUndone, "A move that was expected to be undone has not actually be undone: 3");
            Assert.AreEqual(2, GetGridFullSpacesCount(), "The grid should have only 2 filled spaces but it has " + GetGridFullSpacesCount() + " filled spaces.");
            yield return new WaitForSeconds(DelayInSeconds);

            movesUndone = MatchManager.Instance.UndoLastMoves();
            Assert.Greater(movesUndone.Count, 0, "No moves were undone.");
            Assert.Contains(0, movesUndone, "A move that was expected to be undone has not actually be undone: 0");
            Assert.Contains(1, movesUndone, "A move that was expected to be undone has not actually be undone: 1");
            Assert.AreEqual(0, GetGridFullSpacesCount(), "The grid should be empty but it has " + GetGridFullSpacesCount() + " filled spaces.");
            yield return new WaitForSeconds(DelayInSeconds);
        }

        /// <summary>
        /// Undoes the entire board when it has only 1 space left. Checks if all the undo's worked and that the correct grid nodes were undone each time.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UndoEverythingWhileOnlyOneSpaceLeft()
        {
            /* Creating a grid scenario where there is only one space left
             * X O _
             * X X O
             * O X O
             */

            XOGridNode[] gridNodes = MatchManager.Instance.GridNodes;
            MatchManager.Instance.GridNodeSelected(gridNodes[0]);
            MatchManager.Instance.GridNodeSelected(gridNodes[6]);
            MatchManager.Instance.GridNodeSelected(gridNodes[3]);
            MatchManager.Instance.GridNodeSelected(gridNodes[5]);
            MatchManager.Instance.GridNodeSelected(gridNodes[7]);
            MatchManager.Instance.GridNodeSelected(gridNodes[1]);
            MatchManager.Instance.GridNodeSelected(gridNodes[4]);
            MatchManager.Instance.GridNodeSelected(gridNodes[8]);
            yield return new WaitForSeconds(DelayInSeconds);

            List<int> movesUndone = MatchManager.Instance.UndoLastMoves();
            Assert.Greater(movesUndone.Count, 0, "No moves were undone.");
            Assert.Contains(4, movesUndone, "A move that was expected to be undone has not actually be undone: 4");
            Assert.Contains(8, movesUndone, "A move that was expected to be undone has not actually be undone: 8");
            Assert.AreEqual(6, GetGridFullSpacesCount(), "The grid should have only 6 filled spaces but it has " + GetGridFullSpacesCount() + " filled spaces.");
            yield return new WaitForSeconds(DelayInSeconds);

            movesUndone = MatchManager.Instance.UndoLastMoves();
            Assert.Greater(movesUndone.Count, 0, "No moves were undone.");
            Assert.Contains(1, movesUndone, "A move that was expected to be undone has not actually be undone: 1");
            Assert.Contains(7, movesUndone, "A move that was expected to be undone has not actually be undone: 7");
            Assert.AreEqual(4, GetGridFullSpacesCount(), "The grid should have only 4 filled spaces but it has " + GetGridFullSpacesCount() + " filled spaces.");
            yield return new WaitForSeconds(DelayInSeconds);

            movesUndone = MatchManager.Instance.UndoLastMoves();
            Assert.Greater(movesUndone.Count, 0, "No moves were undone.");
            Assert.Contains(3, movesUndone, "A move that was expected to be undone has not actually be undone: 3");
            Assert.Contains(5, movesUndone, "A move that was expected to be undone has not actually be undone: 5");
            Assert.AreEqual(2, GetGridFullSpacesCount(), "The grid should have only 2 filled spaces but it has " + GetGridFullSpacesCount() + " filled spaces.");
            yield return new WaitForSeconds(DelayInSeconds);

            movesUndone = MatchManager.Instance.UndoLastMoves();
            Assert.Greater(movesUndone.Count, 0, "No moves were undone.");
            Assert.Contains(0, movesUndone, "A move that was expected to be undone has not actually be undone: 0");
            Assert.Contains(6, movesUndone, "A move that was expected to be undone has not actually be undone: 6");
            Assert.AreEqual(0, GetGridFullSpacesCount(), "The grid should be empty but it has " + GetGridFullSpacesCount() + " filled spaces.");
            yield return new WaitForSeconds(DelayInSeconds);
        }

        /// <summary>
        /// Gets the amount of occupied spaces on the match grid
        /// </summary>
        /// <returns>The amount of filled spaces on the grid</returns>
        private int GetGridFullSpacesCount()
        {
            int count = 0;
            foreach (XOGridNode gridNode in MatchManager.Instance.GridNodes)
            {
                if (gridNode.NodeState != 0)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
