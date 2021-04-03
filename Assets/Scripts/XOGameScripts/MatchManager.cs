using System;
using System.Collections;
using System.Collections.Generic;
using TicTacToe;
using UnityEngine;

namespace TicTacToe.XO
{
    public class MatchManager : MonoBehaviour
    {
        #region Data
        public static MatchManager Instance { get; private set; }

        public XOGridNode[] GridNodes = new XOGridNode[9]; // All the active grid node objects. Starts from top left, going through each row from top to bottom.

        // Sprites and renderers. Used by Graphic Bundle Pack
        public Sprite Player1Sprite { get; private set; }
        public Sprite Player2Sprite { get; private set; }
        public Sprite CircleSprite;
        public Sprite CrossSprite;
        public SpriteRenderer BackgroundGraphic;

        // Audio clips
        public AudioClip CrossSound;
        public AudioClip CircleSound;
        public AudioClip VictorySound;
        public AudioClip DefeatSound;
        public AudioClip CursorSound;

        // Public match state variables
        public int CurrentPlayer { get; private set; }
        public int CurrentTurn { get; private set; }
        public XOMatchType MatchType = XOMatchType.LocalMultiplayer;

        // Internal match state variables
        private Stack<int> _previousMovesDone = new Stack<int>();
        private XOMatchState _matchState = XOMatchState.Start;
        private int[] _gridState = new int[9];
        [SerializeField]
        private AILevel _enemyDifficulty = AILevel.Easy;

        // Timers
        [SerializeField]
        private float _timeLimit = 5;
        [SerializeField]
        private float _aiTurnDelay = 0.75f;
        private float _currentTimer = 0;
        private float _currentAIDelay = 0;
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
            }
        }

        private void Start()
        {
            InitializeAssets(); // Load graphic bundle assets on start
        }

        private void Update()
        {
            switch (_matchState)
            {
                case XOMatchState.InProgress:
                    UpdateTimer(); // Update time limit and AI timers

                    switch (MatchType)
                    {
                        case XOMatchType.VsAI:
                            if (CurrentPlayer == 2) // Activate enemy AI only when the current player is player 2.
                            {
                                UpdateEnemyAI(); 
                            }
                            break;
                        case XOMatchType.CpuMatch:
                            UpdateEnemyAI();
                            break;
                    }
                    break;
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Loads the current graphic bundle assets contained in the GameManager
        /// </summary>
        public void InitializeAssets()
        {
            if (GameManager.Instance != null)
            {
                if (!String.IsNullOrEmpty(GameManager.Instance.CurrentGraphicPack.BundleName)) // Don't load any sprites if there is no loaded graphic bundle
                {
                    CrossSprite = GameManager.Instance.CurrentGraphicPack.CrossSymbol;
                    CircleSprite = GameManager.Instance.CurrentGraphicPack.CircleSymbol;
                    BackgroundGraphic.sprite = GameManager.Instance.CurrentGraphicPack.BackgroundGraphic;
                }
            }
        }

        /// <summary>
        /// Sets the MatchType and EnemyDifficult of the match.
        /// </summary>
        /// <param name="matchType">The type of match being played.</param>
        /// <param name="difficulty">The difficulty of the AI in the match.</param>
        public void SetMatchTypeAndDifficulty(XOMatchType matchType, AILevel difficulty)
        {
            MatchType = matchType;
            _enemyDifficulty = difficulty;
        }

        /// <summary>
        /// Start a new match.
        /// </summary>
        /// <param name="randomPlayers">Will randomize player sprites if true.</param>
        /// <param name="skipIntro">Will skip the intro cutscene if true.</param>
        public void StartMatch(bool randomPlayers, bool skipIntro)
        {
            InitializeMatch(randomPlayers); // Reset and initialize all match related variables

            if (skipIntro)
            {
                _matchState = XOMatchState.InProgress;
            }
            else
            {
                StartCoroutine(MatchIntroCutscene()); // Play the opening cutscene coroutine only if we're not skipping the intro.
            }
        }

        /// <summary>
        /// Selects and assigns the approprirate value for the grid node given, dependant on the current player.
        /// </summary>
        /// <param name="selectedNode">The XOGridNode object that was selected.</param>
        public void GridNodeSelected(XOGridNode selectedNode)
        {
            if (_matchState == XOMatchState.InProgress) // Can only select nodes when the game is in progress
            {
                // Update the current turn
                CurrentTurn++;

                // Set the nodes value
                selectedNode.SetNodeValue(CurrentPlayer);
                _gridState[selectedNode.NodeIndex] = CurrentPlayer;

                // Update last turn done by saving it into the stack.
                _previousMovesDone.Push(selectedNode.NodeIndex);

                // Checks if the game has reached an ending.
                EndingType endingType = CheckGameEnd(selectedNode.NodeIndex, _gridState);

                if (endingType != EndingType.None) // If we've reached an ending state, end the match. Otherwise swap the player.
                {
                    EndMatch(endingType);
                }
                else
                {
                    SwapPlayer();
                }
            }
        }

        /// <summary>
        /// Press event for the Restart button
        /// </summary>
        /// <param name="randomPlayers">Randomize players on restart?</param>
        public void RestartMatch(bool randomPlayers)
        {
            // Reset grid values, timer and HUD
            AudioManager.Instance.PlaySoundClip(CursorSound);
            GameManager.Instance.StartGame(MatchType, _enemyDifficulty);
        }

        /// <summary>
        /// Press event for the Hint button
        /// </summary>
        public void HintButtonPress()
        {
            // Only display hint if game is in progress, it's player against AI, and it's the players turn
            if (_matchState == XOMatchState.InProgress && MatchType == XOMatchType.VsAI && CurrentPlayer == 1)
            {
                AudioManager.Instance.PlaySoundClip(CursorSound);
                DisplayHint();
            }
        }

        /// <summary>
        /// Displays a hint for the current active player.
        /// </summary>
        /// <returns>The index of the hinted node.</returns>
        public int DisplayHint()
        {
            int hintedNode = -1;

            // In order to get the best move, we simulate what the enemy AI would do in "impossible" difficulty. 
            // This difficulty will always calculate the best move with the minimax algorithm.
            hintedNode = CalculateAITurn(CurrentPlayer, AILevel.Impossible);

            if (hintedNode != -1)
            {
                GridNodes[hintedNode].FlashHintGraphic(CurrentPlayer); // Displays the hint on the board
            }

            return hintedNode;
        }

        /// <summary>
        /// Press event for the undo button
        /// </summary>
        public void UndoButtonPress()
        {
            // Only undo moves if game is in progress, it's player against AI, and it's the players turn
            if (_matchState == XOMatchState.InProgress && MatchType == XOMatchType.VsAI && CurrentPlayer == 1)
            {
                AudioManager.Instance.PlaySoundClip(CursorSound);
                UndoLastMoves();
            }
        }

        /// <summary>
        /// Press event for the Back button
        /// </summary>
        public void BackButtonPress()
        {
            if (GameManager.Instance != null)
            {
                _matchState = XOMatchState.End;
                AudioManager.Instance.PlaySoundClip(CursorSound);
                GameManager.Instance.GotoScene("MainMenu", TransitionType.Fade);
            }
        }

        /// <summary>
        /// Undoes the player's and the AI's last move. Only works when at least one move was made by both players.
        /// </summary>
        /// <returns>A list containing the two grid indices that had their values reset.</returns>
        public List<int> UndoLastMoves()
        {
            List<int> movesUndone = new List<int>();

            // You can't undo the player and the AI's move if there aren't at least 2 moves in total.
            if (_previousMovesDone.Count >= 2)
            {
                // Undo the previous two turns, the AI's and your own
                CurrentTurn -= 2;

                // Take the last move done from the _previousMovesDone stack and reset the node. Do this twice.
                int lastTurn = _previousMovesDone.Pop();
                _gridState[lastTurn] = 0;
                GridNodes[lastTurn].ResetNode();
                movesUndone.Add(lastTurn);

                lastTurn = _previousMovesDone.Pop();
                _gridState[lastTurn] = 0;
                GridNodes[lastTurn].ResetNode();
                movesUndone.Add(lastTurn);

                // Reset timer
                _currentTimer = _timeLimit;
            }

            return movesUndone;
        }

        /// <summary>
        /// Checks whether the game has ended after the last move has been made.
        /// </summary>
        /// <param name="lastSelectedNode">The index of the last node that was selected by either player.</param>
        /// <returns></returns>
        public EndingType CheckGameEnd(int lastSelectedNode)
        {
            return CheckGameEnd(lastSelectedNode, _gridState); // Defaults to using the real current grid state.
        }

        /// <summary>
        /// Checks whether the game has ended after the last move has been made.
        /// </summary>
        /// <param name="lastSelectedNode">The index of the last node that was selected by either player.</param>
        /// <param name="currentGridState">The current state of the grid to check whether or not it has ended.</param>
        /// <returns>The type of ending reached.</returns>
        public EndingType CheckGameEnd(int lastSelectedNode, int[] currentGridState)
        {
            EndingType endingType = EndingType.None;
            bool isGameEnding = false;

            // Row check
            int rowIndex = (lastSelectedNode / 3) * 3;
            if (currentGridState[rowIndex] == currentGridState[rowIndex + 1] &&
                currentGridState[rowIndex + 1] == currentGridState[rowIndex + 2])
            {
                isGameEnding = true;
            }

            // Continue only if row check failed
            if (!isGameEnding)
            {
                // Column check
                int columnIndex = lastSelectedNode % 3;
                if (currentGridState[columnIndex] == currentGridState[columnIndex + 3] &&
                    currentGridState[columnIndex + 3] == currentGridState[columnIndex + 6])
                {
                    isGameEnding = true;
                }

                // Continue only if column check failed
                if (!isGameEnding)
                {
                    // Diagonal check /
                    if (lastSelectedNode == 2 || lastSelectedNode == 4 || lastSelectedNode == 6)
                    {
                        if (currentGridState[2] == currentGridState[4] &&
                            currentGridState[4] == currentGridState[6])
                        {
                            isGameEnding = true;
                        }
                    }

                    // Continue only if diagonal / check failed
                    if (!isGameEnding)
                    {
                        // Diagonal check \
                        if (lastSelectedNode == 0 || lastSelectedNode == 4 || lastSelectedNode == 8)
                        {
                            if (currentGridState[0] == currentGridState[4] &&
                                currentGridState[4] == currentGridState[8])
                            {
                                isGameEnding = true;
                            }
                        }
                        // Continue only if diagonal \ check failed
                        if (!isGameEnding)
                        {
                            // Check for a draw (The board will always be full at turn 10)
                            if (CurrentTurn == 10)
                            {
                                endingType = EndingType.Draw;
                                isGameEnding = true;
                            }
                            else
                            {
                                // Check for a draw by looking if all the nodes are filled
                                bool emptyTileFound = false;
                                for (int i = 0; i < 9; i++)
                                {
                                    if (currentGridState[i] == 0)
                                    {
                                        emptyTileFound = true;
                                        break;
                                    }
                                }

                                if (!emptyTileFound)
                                {
                                    endingType = EndingType.Draw;
                                    isGameEnding = true;
                                }
                            }
                        }
                    }
                }
            }

            if (endingType != EndingType.Draw && isGameEnding) // If the game is going to end and it's not a draw, that means someone won.
            {
                // The last selected nodes value will either be 1 (stands for Player 1) or 2 (stands for Player 2).
                // The EndingType enum has its value assigned in such a way that the value of '1' Means Player 1 wins, and the value of '2' means that Player 2 wins.
                endingType = (EndingType)currentGridState[lastSelectedNode];
            }

            return endingType;
        }

        /// <summary>
        /// Coroutine to play the opening cutscene of the match.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MatchIntroCutscene()
        {
            yield return new WaitForSeconds(0.2f);
            XOHUDManager.Instance.DisplayIntro(CurrentPlayer);
            yield return new WaitForSeconds(2f);
            _matchState = XOMatchState.InProgress;
        }

        /// <summary>
        /// Resets and intializes all match related variables. Also sets up the HUD and player sprites.
        /// </summary>
        /// <param name="randomPlayers">Will randomize the player sprites if true.</param>
        private void InitializeMatch(bool randomPlayers)
        {
            // Reset timers
            _currentTimer = _timeLimit;
            _currentAIDelay = _aiTurnDelay;

            // Reset current match state, grid, undo stack and sets turn back to 1.
            _matchState = XOMatchState.Start;
            _gridState = new int[9];
            _previousMovesDone = new Stack<int>();
            CurrentTurn = 1;

            // If we're randomizing players, there'll be a 50/50 chance on who starts the match (Cross always starts first).
            // If we're not randomizing, player 1 will always be Cross and will start first.
            float randomizeChance = randomPlayers ? 50f : 0f;
            if (UnityEngine.Random.Range(0f, 100f) >= randomizeChance) // 50% chance for player 1 to start as Cross
            {
                CurrentPlayer = 1;
                Player1Sprite = CrossSprite;
                Player2Sprite = CircleSprite;
            }
            else
            {
                CurrentPlayer = 2;
                Player1Sprite = CircleSprite;
                Player2Sprite = CrossSprite;
            }

            // Reset all grid node graphics and values.
            foreach (XOGridNode gridNode in GridNodes)
            {
                gridNode.ResetNode();
            }

            // Reset HUD
            XOHUDManager.Instance.InitializeHUD();
        }

        /// <summary>
        /// Updates the time limit timer.
        /// </summary>
        private void UpdateTimer()
        {
            _currentTimer -= Time.deltaTime;

            XOHUDManager.Instance.UpdateTimeLimit(_currentTimer); // Updates time limit display on-screen

            if (_currentTimer <= 0)
            {
                // End match when timer hits 0. The winning player is the opposite of the current player.
                int winningPlayer = CurrentPlayer == 1 ? 2 : 1;

                EndMatch((EndingType)winningPlayer);
            }
        }

        /// <summary>
        /// Updates the enemy AI timer.
        /// </summary>
        private void UpdateEnemyAI()
        {
            if (_currentAIDelay > 0)
            {
                _currentAIDelay -= Time.deltaTime;
            }
            else
            {
                // Once AI delay hits 0 or lower, calculate the next AI move and select it.
                int nextTurn = CalculateAITurn(CurrentPlayer, _enemyDifficulty); // Function returns -1 if it encounters an error
                if (nextTurn != -1)
                {
                    GridNodeSelected(GridNodes[nextTurn]);
                }
            }
        }

        /// <summary>
        /// Calculates the move that the AI will take. It will either return it's best possible option, or a completely random one.
        /// It will always return a random node if this is the first move.
        /// The chance of the method returning a random node is determined by the AI level.
        /// Easy: 100% Random
        /// Medium: 50% Random
        /// Hard: 10% Random
        /// Impossible: 0% Random
        /// </summary>
        /// <param name="currentPlayer">The current acting player.</param>
        /// <param name="aiLevel">The level of the AI doing the calculation.</param>
        /// <returns></returns>
        private int CalculateAITurn(int currentPlayer, AILevel aiLevel)
        {
            int nextTurn = -1;

            if (CurrentTurn == 1) // Always return a random node on the first move.
            {
                nextTurn = GetRandomEmptyGridNode();
            }
            else
            {
                // Easy: 100% Random
                // Medium: 50% Random
                // Hard: 10% Random
                // Impossible: 0% Random
                float randomNodeProbability = 0;
                switch (aiLevel)
                {
                    case AILevel.Easy:
                        randomNodeProbability = 100;
                        break;
                    case AILevel.Medium:
                        randomNodeProbability = 50;
                        break;
                    case AILevel.Hard:
                        randomNodeProbability = 10;
                        break;
                    case AILevel.Impossible:
                        randomNodeProbability = 0;
                        break;
                }

                if (UnityEngine.Random.Range(0f, 100f) > randomNodeProbability)
                {
                    nextTurn = GetBestMove(currentPlayer); // Returns the best possible move using the minimax algorithm.
                }
            }

            if (nextTurn == -1) // The game ends in a draw if the function returns -1, so just select any random node
            {
                nextTurn = GetRandomEmptyGridNode();
            }

            return nextTurn;
        }

        /// <summary>
        /// Retrieves a random empty grid node's index
        /// </summary>
        /// <returns>The index of a random empty grid node.</returns>
        private int GetRandomEmptyGridNode()
        {
            int randomNode = -1;
            List<int> possibleOptions = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                if (_gridState[i] == 0)
                {
                    possibleOptions.Add(i);
                }
            }

            if (possibleOptions.Count > 0)
            {
                randomNode = possibleOptions[UnityEngine.Random.Range(0, possibleOptions.Count)];
            }

            return randomNode;
        }

        /// <summary>
        /// Returns the grid index of the best possible move the currentPlayer can make, using the minimax algorithm.
        /// </summary>
        /// <param name="currentPlayer">The current acting player.</param>
        /// <returns>The grid index of the best move for the currentPlayer.</returns>
        private int GetBestMove(int currentPlayer)
        {
            int bestScore = -999; // The "score" of the move that will result in the most favorable outcome.
            int bestMove = -1; // The index of the node which will result in the best move.

            // Go over all the grid nodes
            for (int i = 0; i < 9; i++)
            {
                // Check if this grid spot is empty
                if (_gridState[i] == 0)
                {
                    // Copy the board and place the current players move at grid space 'i'
                    int[] newBoard = new int[9];
                    Array.Copy(_gridState, newBoard, 9);
                    newBoard[i] = currentPlayer;

                    // Calculate the score for this next move, and save the highest one in order to return the best move
                    int scoreForNextMove = GetMinimaxValue(newBoard, 1, false, i, currentPlayer == 1 ? 2 : 1);
                    if (scoreForNextMove > bestScore)
                    {
                        bestScore = scoreForNextMove;
                        bestMove = i;
                    }
                }
            }

            return bestMove;
        }

        /// <summary>
        /// Returns the "score" for the current situation by calculating all possible options that can be done by the current player and their opponnent.
        /// The higher the score, the more favorable the board situation is. The lower the score, the less favorable the board situation is.
        /// </summary>
        /// <param name="currentBoard">The current state of the match board.</param>
        /// <param name="depth">How many times was this function called within itself.</param>
        /// <param name="isMaximizing">When maximizing, the function will look to try and get the best score for the original player, and the opposite is done when minimizing.</param>
        /// <param name="lastSelectedNode">The index of the node selected on the previous turn.</param>
        /// <param name="currentPlayer">The current player acting in the simulation.</param>
        /// <returns>The score for the current board situation. Will be positive if the outcome is good for the current player, or negative if it's bad. 0 if it ends in a draw.</returns>
        private int GetMinimaxValue(int[] currentBoard, int depth, bool isMaximizing, int lastSelectedNode, int currentPlayer)
        {
            EndingType endingType = CheckGameEnd(lastSelectedNode, currentBoard); // Check if the game has ended when the previous move was made.

            if (endingType != EndingType.None)
            {
                // endingType is 1 if player1 wins, and its 2 if player2 wins. CurrentPlayer represents the player who is calculating their best move,
                // so if the winner is the same player as the one calculating their best move, this current move gets 10 point, otherwise it gets -10 points.
                // The "deeper" the calculation goes, the less valuable it is, and that is why this point value gets reduced by the depth.
                if (endingType == EndingType.Draw)
                {
                    return 0;
                }
                else
                {
                    return ((int)endingType == CurrentPlayer ? 10 : -10) / depth;
                }
            }

            // When maximizing, the algorithm will look for the best possible score for the acting player in the current given board state.
            if (isMaximizing)
            {
                int bestScore = -999;

                // Go over all spaces on the grid
                for (int i = 0; i < 9; i++)
                {
                    // Is this spot empty?
                    if (currentBoard[i] == 0)
                    {
                        // Copy the board and place the current players move at grid space 'i'
                        int[] newBoard = new int[9];
                        Array.Copy(currentBoard, newBoard, 9);
                        newBoard[i] = currentPlayer;

                        // Get the score for the next simulated move and save the highest score.
                        int scoreForNextMove = GetMinimaxValue(newBoard, depth + 1, false, i, currentPlayer == 1 ? 2 : 1);
                        bestScore = Mathf.Max(scoreForNextMove, bestScore);
                    }
                }

                return bestScore;
            }
            else // When minimizing, the algorithm will look for the best possible score for the OPPOSITE acting player in the current given board state.
            {
                int bestScore = 999;

                // Go over all spaces on the grid
                for (int i = 0; i < 9; i++)
                {
                    // Is this spot empty?
                    if (currentBoard[i] == 0)
                    {
                        // Copy the board and place the current players move at grid space 'i'
                        int[] newBoard = new int[9];
                        Array.Copy(currentBoard, newBoard, 9);
                        newBoard[i] = currentPlayer;

                        // Get the score for the next simulated move and save the lowest score.
                        int scoreForNextMove = GetMinimaxValue(newBoard, depth + 1, true, i, currentPlayer == 1 ? 2 : 1);
                        bestScore = Mathf.Min(scoreForNextMove, bestScore);
                    }
                }

                return bestScore;
            }
        }

        /// <summary>
        /// Swaps the current player variable with the opposite player.
        /// Resets the time limit and updates the player HUD.
        /// </summary>
        private void SwapPlayer()
        {
            // Swap turn, update HUD
            CurrentPlayer = CurrentPlayer == 1 ? 2 : 1;
            XOHUDManager.Instance.SetActivePlayerHUD(CurrentPlayer);

            // Reset timer
            _currentTimer = _timeLimit;

            // Setup enemy AI's turn if match is against AI
            if (MatchType != XOMatchType.LocalMultiplayer)
            {
                _currentAIDelay = _aiTurnDelay;
            }
        }

        /// <summary>
        /// Ends the match. The animation and sound played depends on the type of ending.
        /// </summary>
        /// <param name="endingType">The type of ending the match reached.</param>
        private void EndMatch(EndingType endingType)
        {
            _matchState = XOMatchState.End;

            // The sound clip will be a "Defeat" if we're playing against AI and the AI won. In every other case, play a "Victory" sound.
            AudioClip endingSound = (MatchType == XOMatchType.VsAI && endingType == EndingType.Player2Win) ? DefeatSound : VictorySound;

            AudioManager.Instance.PlaySoundClip(endingSound);
            XOHUDManager.Instance.DisplayMatchEndingText(endingType);
        }
        #endregion
    }
}