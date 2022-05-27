using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Utils;
using Debug = UnityEngine.Debug;

namespace Core {
    public class Game : MonoBehaviour {
        public BoardUI boardUi;
        public List<Move> currentPseudoLegalMoves;
        public List<Move> currentAvailableMoves;
        public static bool GameIsPaused = false;
        
        public GameObject sideToPlayText;
        public GameObject lastMoveText;
        public GameObject evaluationText;
        public GameObject gameSituationText;
        public GameObject bToUndoText;
        public GameObject file1Text;
        public GameObject file2Text;
        public GameObject file3Text;
        public GameObject file4Text;
        public GameObject file5Text;
        public GameObject file6Text;
        public GameObject file7Text;
        public GameObject file8Text;
        public GameObject rank1Text;
        public GameObject rank2Text;
        public GameObject rank3Text;
        public GameObject rank4Text;
        public GameObject rank5Text;
        public GameObject rank6Text;
        public GameObject rank7Text;
        public GameObject rank8Text;
        public GameObject gameStatusText;
        public GameObject bestMoveText;
        public GameObject moveReactionText;
        private bool gameEnded = false;
        private bool hasTurnChanged = false;
        private bool undoPromptEnabled = false;
        private int movesMade = 0;
        private float prevEvaluation = 0;
        private SearchInfo searchInfo;
        
        private Camera cam;
        private int selectedRank;
        private int selectedFile;
        private int lastSelectedIndex = -1;
        private int playerSide = Board.White;
        private Move lastPlayedMove = null;
        private PlayerType currentPlayer = PlayerType.Human;
        public float currentPlayerEvaluation;
        public Move suggestedMove = null;

        public enum InputState {
            None,
            PieceSelected
        }

        public enum PlayerType {
            Human,
            AI
        }

        private Board mainBoard;
        private InputState currentState;
        private PromotionMenu promotionMenu;
        private SearchInfo searchParameters;

        private void Start() {
            boardUi = FindObjectOfType<BoardUI>();
            mainBoard = new Board();
            promotionMenu = FindObjectOfType<PromotionMenu>();
            InitializeGame();
            cam = Camera.main;
            currentState = InputState.None;
            InitSearchInfo();
        }
        
        private void Update() {
            if (!GameIsPaused) {
                if (currentPseudoLegalMoves != null && currentPlayer == PlayerType.Human) HandleInput();
                else if (currentPseudoLegalMoves != null && currentPlayer == PlayerType.AI && !gameEnded && hasTurnChanged) AISearchAndMakeMove();
            }
        }
        
        private void InitializeGame() {
            mainBoard.LoadStartingPosition();
            boardUi.UpdateBoard(mainBoard);
            boardUi.ResetSquareColors();
            currentPseudoLegalMoves = MoveGenerator.GenerateAllMoves(mainBoard);
            if (GameSettings.GameMode == GameSettings.GameModeEnum.LearningMode && currentPlayer == PlayerType.Human) {
                GetQuickEvaluation();
            }
        }

        public void SwitchSides() {
            if (playerSide == Board.White) {
                boardUi.SetPerspective(false);
                playerSide = Board.Black;
                currentPlayer = PlayerType.AI;
                boardUi.isBottomWhite = false;
            } else {
                boardUi.SetPerspective(true);
                playerSide = Board.White;
                currentPlayer = PlayerType.Human;
                boardUi.isBottomWhite = true;
            }
            SetFilesRanksText();
            InitializeGame();
            gameEnded = false;
        }
        
        IEnumerator ChangePlayer() {
            yield return new WaitForSeconds (0.1f);
            currentPlayer = currentPlayer == PlayerType.Human ? PlayerType.AI : PlayerType.Human;
            hasTurnChanged = true;
        }

        IEnumerator TestMoveGenerator(List<Move> moveList) {
            foreach (Move move in moveList) {
                if (!mainBoard.MakeMove(move)) {
                    continue;
                }
                Debug.Log("Made move: " + BoardSquares.GetAlgebraicMove(move.move));
                mainBoard.PrintGameBoard();
                boardUi.UpdateBoard(mainBoard);
                yield return new WaitForSeconds (1.0f);
                mainBoard.UnmakeMove();
                Debug.Log("Unmade move: " + BoardSquares.GetAlgebraicMove(move.move));
                mainBoard.PrintGameBoard();
                boardUi.UpdateBoard(mainBoard);
            }
        }

        private void HandleInput() {
            Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0)) {
                if (currentState == InputState.None) {
                    AttemptSelectPiece(mousePosition);
                } else {
                    CheckSquareSelected(mousePosition);
                }
            } /*else if (Input.GetKeyDown(KeyCode.P)) {
                int max = mainBoard.pvTable.GetPvLineCount(6);
                Debug.Log("PV line of " + max + " moves.");
                for (int pvNum = 0; pvNum < max; pvNum++) {
                    Move move = mainBoard.pvArray[pvNum];
                    Debug.Log("PV move " + pvNum + ": " + Move.GetMoveString(move));
                }
            }*/ else if (Input.GetKeyDown(KeyCode.B) && GameSettings.GameMode == GameSettings.GameModeEnum.LearningMode) {
                searchParameters.stopped = true;
                UnmakeMove();
            } /*else if (Input.GetKeyDown(KeyCode.M) ) {
                mainBoard.PrintMirrorBoardEvaluations();
            }*/
        }

        private void CheckSquareSelected(Vector2 mousePosition) {
            if (boardUi.TryGetSquareUnderMouse(mousePosition, out selectedFile, out selectedRank)) {
                int index = Board.FrTo120Sq(selectedFile, selectedRank);
                if (index != lastSelectedIndex) {
                    boardUi.ResetSquareColors();
                    int color = Piece.GetColor(mainBoard.squares[index]);
                    if (color != mainBoard.sideToPlay) {
                        int moveIndex = currentAvailableMoves.FindIndex(move => Move.ToSquare(move.move) == index);
                        if (moveIndex >= 0) {
                            Move moveToMake = currentAvailableMoves[moveIndex];
                            if (!Move.IsMovePromotion(moveToMake.move)) {
                                MakeMove(moveToMake);
                            } else {
                                promotionMenu.SelectPromotionMove(moveToMake, mainBoard.sideToPlay,this);
                            }
                        } 
                    } else {
                        lastSelectedIndex = index;
                        Coordinates coordinates = BoardSquares.CoordFromIndex(Board.Sq64(index));
                        boardUi.SetSelectedColor(coordinates);
                        currentAvailableMoves = mainBoard.GetLegalMovesForSquare(index, currentPseudoLegalMoves);
                        boardUi.HighlightLegalMoves(currentAvailableMoves.ToArray());
                        currentState = InputState.PieceSelected;
                    }
                } else {
                    boardUi.ResetSquareColors();
                    lastSelectedIndex = -1;
                }
            }
        }

        private void MakeMove(Move move) {
            gameStatusText.GetComponent<ShowText>().textValue = "";
            boardUi.ResetSquareColors();
            mainBoard.MakeMove(move);
            movesMade++;
            Coordinates fromCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.FromSquare(move.move)));
            Coordinates toCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.ToSquare(move.move)));
            boardUi.SetHighlightColor(fromCoordinates);
            boardUi.SetHighlightColor(toCoordinates);
            boardUi.UpdateBoard(mainBoard);
            if (GameSettings.GameMode == GameSettings.GameModeEnum.LearningMode) {
                LearningModeUI(move);
            }
            currentState = InputState.None;
            lastPlayedMove = move;
            ChangeTurn();
        }

        private void GetQuickEvaluation() {
            SearchInfo searchParameters2 = new SearchInfo(depth: 7, timeSet: true, durationSet: 0.1f, quiescence: true, transposition: true);
            Search.SearchPosition(mainBoard, searchParameters2, this, nullMove: true, printAllData: false);
        }

        private void StoreBestMove() {
            suggestedMove = mainBoard.pvArray[1];
        }

        private void AttemptSelectPiece(Vector2 mousePosition) {
            if (boardUi.TryGetSquareUnderMouse(mousePosition, out selectedFile, out selectedRank)) {
                int index = Board.FrTo120Sq(selectedFile, selectedRank);
                int color = Piece.GetColor(mainBoard.squares[index]);
                if (color == mainBoard.sideToPlay) {
                    lastSelectedIndex = index;
                    Coordinates coordinates = BoardSquares.CoordFromIndex(Board.Sq64(index));
                    boardUi.SetSelectedColor(coordinates);
                    currentAvailableMoves = mainBoard.GetLegalMovesForSquare(index, currentPseudoLegalMoves);
                    if (currentAvailableMoves.Count > 0) {
                        boardUi.HighlightLegalMoves(currentAvailableMoves.ToArray());
                        currentState = InputState.PieceSelected;
                    }
                }
            }
        }

        public void MakePromotionMove(Move move) {
            mainBoard.MakeMove(move);
            Coordinates fromCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.FromSquare(move.move)));
            Coordinates toCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.ToSquare(move.move)));
            boardUi.SetHighlightColor(fromCoordinates);
            boardUi.SetHighlightColor(toCoordinates);
            boardUi.UpdateBoard(mainBoard);
            if (GameSettings.GameMode == GameSettings.GameModeEnum.LearningMode) {
                LearningModeUI(move);
            }
            currentState = InputState.None;
            lastPlayedMove = move;
            ChangeTurn();
        }

        public void UnmakeMove() {
            if (mainBoard.histPly > 1) {
                mainBoard.UnmakeMove();
                mainBoard.UnmakeMove();
                boardUi.ResetSquareColors();
                if (mainBoard.histPly != 0) {
                    Move previousMove = new Move(mainBoard.gameHist[mainBoard.histPly - 1].move, 0);
                    Coordinates fromCoordinates =
                        BoardSquares.CoordFromIndex(Board.Sq64(Move.ToSquare(previousMove.move)));
                    Coordinates toCoordinates =
                        BoardSquares.CoordFromIndex(Board.Sq64(Move.FromSquare(previousMove.move)));
                    boardUi.SetHighlightColor(fromCoordinates);
                    boardUi.SetHighlightColor(toCoordinates);
                    lastPlayedMove = previousMove;
                } else {
                    lastPlayedMove = null;
                }
                boardUi.UpdateBoard(mainBoard);
                currentState = InputState.None;
                ChangeTurn(changePlayer: false);
            }
        }

        public void ChangeTurn(bool changePlayer = true) {
            currentPseudoLegalMoves = MoveGenerator.GenerateAllMoves(mainBoard);
            CheckEnding();
            if (GameSettings.GameMode == GameSettings.GameModeEnum.LearningMode) {
                if (!undoPromptEnabled && currentPlayer == PlayerType.AI) {
                    bToUndoText.GetComponent<ShowText>().textValue = "Press B to undo your last move.";
                    undoPromptEnabled = true;
                } else {
                    bToUndoText.GetComponent<ShowText>().textValue = "";
                }
                string plusModifier = currentPlayerEvaluation >= 0f ? "+" : "";
                evaluationText.GetComponent<ShowText>().textValue = plusModifier + String.Format(currentPlayerEvaluation % 1 == 0 ? "{0:0}" : "{0:0.00}", currentPlayerEvaluation);
                if (currentPlayerEvaluation >= 1.0f) {
                    evaluationText.GetComponent<Text>().color = Color.green;
                    gameSituationText.GetComponent<ShowText>().textValue = playerSide == Board.White ? "White winning!" : "Black winning!";
                } else if (currentPlayerEvaluation >= -1.0f) {
                    evaluationText.GetComponent<Text>().color = Color.white;
                    gameSituationText.GetComponent<ShowText>().textValue = "Close game!";
                } else {
                    evaluationText.GetComponent<Text>().color = Color.red;
                    gameSituationText.GetComponent<ShowText>().textValue = playerSide == Board.White ? "Black winning!" : "White winning!";
                }
            }
            string lastPlayedMoveString =
                lastPlayedMove == null ? "-" : BoardSquares.GetAlgebraicMove(lastPlayedMove.move);
            sideToPlayText.GetComponent<ShowText>().textValue = GetSideToPlayString(mainBoard.sideToPlay);
            lastMoveText.GetComponent<ShowText>().textValue = "Last move: " + lastPlayedMoveString;
            if (changePlayer) StartCoroutine(ChangePlayer());
            //mainBoard.PrintPawnAnalysis();
        }

        private void LearningModeUI(Move move) {
            if (currentPlayer == PlayerType.Human) {
                prevEvaluation = currentPlayerEvaluation;
                GetQuickEvaluation();
                if (movesMade > 4) PrintBestMove();
                if (movesMade > 1) {
                    if (prevEvaluation - currentPlayerEvaluation > 1.5f) PrintBlunderMove(move);
                    else if (prevEvaluation - currentPlayerEvaluation < -1.0f || move.move == suggestedMove.move) GreatMove();
                }
            } else if (currentPlayer == PlayerType.AI) {
                StoreBestMove();
                bestMoveText.GetComponent<ShowText>().textValue = "";
                moveReactionText.GetComponent<ShowText>().textValue = "";
            }
        }

        private void PrintBestMove() {
            Coordinates fromCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.ToSquare(suggestedMove.move)));
            Coordinates toCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.FromSquare(suggestedMove.move)));
            boardUi.SetSuggestedColor(fromCoordinates);
            boardUi.SetSuggestedColor(toCoordinates);
            bestMoveText.GetComponent<ShowText>().textValue = "Best move: " + BoardSquares.GetAlgebraicMove(suggestedMove.move);
        }

        private void PrintBlunderMove(Move move) {
            Coordinates fromCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.ToSquare(move.move)));
            Coordinates toCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.FromSquare(move.move)));
            boardUi.SetBlunderColor(fromCoordinates);
            boardUi.SetBlunderColor(toCoordinates);
            moveReactionText.GetComponent<Text>().color = Color.red;
            moveReactionText.GetComponent<ShowText>().textValue = "Blunder!";
        }

        private void GreatMove() {
            moveReactionText.GetComponent<Text>().color = new Color(0.49f, 0, 1f, 1f);
            moveReactionText.GetComponent<ShowText>().textValue = "Great move!";
        }

        private void AISearchAndMakeMove() {
            hasTurnChanged = false;
            //searchParameters = new SearchInfo(depth: 20, timeSet: true, durationSet: 10, quiescence: true, transposition: true);
            Search.SearchPosition(mainBoard, searchParameters, this, nullMove: true, printAllData: true);
            Move bestMove = mainBoard.pvArray[0];
            StartCoroutine(WaitAndMakeMove(bestMove));
            //MakeMove(bestMove);
        }

        public IEnumerator WaitAndMakeMove(Move move) {
            float timeToWait = searchParameters.durationSet - searchParameters.realDuration;
            Debug.Log("Time to wait: " + timeToWait);
            yield return new WaitForSeconds(timeToWait);
            MakeMove(move);
        }

        private void InitSearchInfo() {
            switch (GameSettings.Difficulty) {
                case GameSettings.DifficultyEnum.Beginner:
                    searchParameters = new SearchInfo(depth: 1, true, 5, quiescence: false, transposition: false);
                    break;
                case GameSettings.DifficultyEnum.Amateur:
                    searchParameters = new SearchInfo(depth: 3, true, 5, quiescence: false, transposition: false);
                    break;
                case GameSettings.DifficultyEnum.Intermediate:
                    searchParameters = new SearchInfo(depth: 5, true, 5, quiescence: true, transposition: true);
                    break;
                case GameSettings.DifficultyEnum.Hard:
                    searchParameters = new SearchInfo(depth: 7, true, 5, quiescence: true, transposition: true);
                    break;
                case GameSettings.DifficultyEnum.Master:
                    searchParameters = new SearchInfo(depth: 20, true, 10, quiescence: true, transposition: true);
                    break;
            }
        }

        private string GetSideToPlayString(int sideToPlay) {
            return sideToPlay == Piece.White ? "Side to play: White" : "Side to play: Black";
        }

        private string GetCastlingRightsString(int castlingRights) {
            string returnString = "Castling rights: ";
            returnString += (castlingRights & (int) CastlingRightsEnum.WKCA) != 0 ? "K" : "-";
            returnString += (castlingRights & (int) CastlingRightsEnum.WQCA) != 0 ? "Q" : "-";
            returnString += (castlingRights & (int) CastlingRightsEnum.BKCA) != 0 ? "k" : "-";
            returnString += (castlingRights & (int) CastlingRightsEnum.BQCA) != 0 ? "q" : "-";
            return returnString;
        }

        private void CheckEnding() {
            List<Move> currentLegalMoves = mainBoard.GetLegalMovesInPosition(currentPseudoLegalMoves);
            bool isKingIncheck = mainBoard.IsKingInCheck();
            if (currentLegalMoves.Count == 0) {
                if (isKingIncheck) {
                    Checkmate();
                    gameEnded = true;
                } else {
                    Stalemate();
                } 
            } else if (isKingIncheck) {
                Check();
            }
        }

        private void Checkmate() {
            Debug.Log("Checkmate");
            gameStatusText.GetComponent<ShowText>().textValue = "Checkmate!";
        }

        private void Stalemate() {
            Debug.Log("Stalemate");
            gameStatusText.GetComponent<ShowText>().textValue = "Stalemate!";
        }

        private void Check() {
            Debug.Log("Check");
            gameStatusText.GetComponent<ShowText>().textValue = "Check!";
        }

        public void QuitToMainMenu() {
            SceneManager.LoadScene("MainMenu");
        }
        
        private void SetFilesRanksText() {
            if (playerSide == Board.White) {
                file1Text.GetComponent<ShowText>().textValue = "A";
                file2Text.GetComponent<ShowText>().textValue = "B";
                file3Text.GetComponent<ShowText>().textValue = "C";
                file4Text.GetComponent<ShowText>().textValue = "D";
                file5Text.GetComponent<ShowText>().textValue = "E";
                file6Text.GetComponent<ShowText>().textValue = "F";
                file7Text.GetComponent<ShowText>().textValue = "G";
                file8Text.GetComponent<ShowText>().textValue = "H";
                rank1Text.GetComponent<ShowText>().textValue = "1";
                rank2Text.GetComponent<ShowText>().textValue = "2";
                rank3Text.GetComponent<ShowText>().textValue = "3";
                rank4Text.GetComponent<ShowText>().textValue = "4";
                rank5Text.GetComponent<ShowText>().textValue = "5";
                rank6Text.GetComponent<ShowText>().textValue = "6";
                rank7Text.GetComponent<ShowText>().textValue = "7";
                rank8Text.GetComponent<ShowText>().textValue = "8";
            } else {
                file1Text.GetComponent<ShowText>().textValue = "H";
                file2Text.GetComponent<ShowText>().textValue = "G";
                file3Text.GetComponent<ShowText>().textValue = "F";
                file4Text.GetComponent<ShowText>().textValue = "E";
                file5Text.GetComponent<ShowText>().textValue = "D";
                file6Text.GetComponent<ShowText>().textValue = "C";
                file7Text.GetComponent<ShowText>().textValue = "B";
                file8Text.GetComponent<ShowText>().textValue = "A";
                rank1Text.GetComponent<ShowText>().textValue = "8";
                rank2Text.GetComponent<ShowText>().textValue = "7";
                rank3Text.GetComponent<ShowText>().textValue = "6";
                rank4Text.GetComponent<ShowText>().textValue = "5";
                rank5Text.GetComponent<ShowText>().textValue = "4";
                rank6Text.GetComponent<ShowText>().textValue = "3";
                rank7Text.GetComponent<ShowText>().textValue = "2";
                rank8Text.GetComponent<ShowText>().textValue = "1";
            }
        }
    }
}