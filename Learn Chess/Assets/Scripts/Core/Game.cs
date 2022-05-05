using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UI;
using UnityEngine;
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
        
        private GameObject sideToPlayText;
        private GameObject enPassantText;
        private GameObject castlingRightsText;
        private GameObject lastMoveText;
        private GameObject file1Text;
        private GameObject file2Text;
        private GameObject file3Text;
        private GameObject file4Text;
        private GameObject file5Text;
        private GameObject file6Text;
        private GameObject file7Text;
        private GameObject file8Text;
        private GameObject rank1Text;
        private GameObject rank2Text;
        private GameObject rank3Text;
        private GameObject rank4Text;
        private GameObject rank5Text;
        private GameObject rank6Text;
        private GameObject rank7Text;
        private GameObject rank8Text;
        private GameObject gameStatusText;
        
        private Camera cam;
        private int selectedRank;
        private int selectedFile;
        private int lastSelectedIndex = -1;
        private int playerSide = Board.White;
        private Move lastPlayedMove = null;
        private PlayerType currentPlayer = PlayerType.Human;

        public enum InputState {
            None,
            PieceSelected
        }

        public enum PlayerType
        {
            Human,
            AI
        }

        private Board mainBoard;
        private InputState currentState;
        private PromotionMenu promotionMenu;

        private void Start() {
            sideToPlayText = GameObject.Find("SideToPlayText");
            enPassantText = GameObject.Find("EnPassantText");
            castlingRightsText = GameObject.Find("CastlingRightsText");
            lastMoveText = GameObject.Find("LastMoveText");
            file1Text = GameObject.Find("File1");
            file2Text = GameObject.Find("File2");
            file3Text = GameObject.Find("File3");
            file4Text = GameObject.Find("File4");
            file5Text = GameObject.Find("File5");
            file6Text = GameObject.Find("File6");
            file7Text = GameObject.Find("File7");
            file8Text = GameObject.Find("File8");
            rank1Text = GameObject.Find("Rank1");
            rank2Text = GameObject.Find("Rank2");
            rank3Text = GameObject.Find("Rank3");
            rank4Text = GameObject.Find("Rank4");
            rank5Text = GameObject.Find("Rank5");
            rank6Text = GameObject.Find("Rank6");
            rank7Text = GameObject.Find("Rank7");
            rank8Text = GameObject.Find("Rank8");
            gameStatusText = GameObject.Find("GameStatusText");
            boardUi = FindObjectOfType<BoardUI>();
            mainBoard = new Board();
            promotionMenu = FindObjectOfType<PromotionMenu>();
            InitializeGame();
            cam = Camera.main;
            currentState = InputState.None;
        }
        
        private void InitializeGame() {
            /*mainBoard.LoadStartingPosition();
            boardUi.UpdateBoard(mainBoard);
            mainBoard.LoadPosition(FenDecoder.DecodePositionFromFen(Constants.whitePawnMovesFen));
            mainBoard.ShowSquaresAttackedBySide(Board.White);
            mainBoard.ShowSquaresAttackedBySide(Board.Black);
            boardUi.UpdateBoard(mainBoard);
            List<Move> moveList =  MoveGenerator.GenerateAllMoves(mainBoard);
            MoveGenerator.PrintMoveList(moveList);*/
            
            mainBoard.LoadStartingPosition();
            //mainBoard.LoadPosition(FenDecoder.DecodePositionFromFen(Constants.perftTestingFen));
            boardUi.UpdateBoard(mainBoard);
            boardUi.ResetSquareColors();
            //PerftTesting.PerftTest(mainBoard, 4);
            /*List<Move> moveList = MoveGenerator.GenerateAllMoves(mainBoard);
            StartCoroutine(TestMoveGenerator(moveList));*/
            currentPseudoLegalMoves = MoveGenerator.GenerateAllMoves(mainBoard);
        }

        public void SwitchSides() {
            if (playerSide == Board.White)
            {
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

        private void Update() {
            if (!GameIsPaused)
            {
                string lastPlayedMoveString =
                    lastPlayedMove == null ? "-" : BoardSquares.GetAlgebraicMove(lastPlayedMove.move);
                sideToPlayText.GetComponent<ShowText>().textValue = GetSideToPlayString(mainBoard.sideToPlay);
                enPassantText.GetComponent<ShowText>().textValue =
                    "En Passant square: " + BoardSquares.SquareNameDictionary[mainBoard.enPassantSquare];
                castlingRightsText.GetComponent<ShowText>().textValue = GetCastlingRightsString(mainBoard.castlingRights);
                lastMoveText.GetComponent<ShowText>().textValue = "Last move: " + lastPlayedMoveString;
                if (currentPseudoLegalMoves != null && currentPlayer == PlayerType.Human) HandleInput();
                //else if (currentPseudoLegalMoves != null && currentPlayer == PlayerType.AI) AISearchAndMakeMove();
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
            } else if (Input.GetKeyDown(KeyCode.P)) {
                int max = mainBoard.pvTable.GetPvLineCount(6);
                Debug.Log("PV line of " + max + " moves.");
                for (int pvNum = 0; pvNum < max; pvNum++) {
                    Move move = mainBoard.pvArray[pvNum];
                    Debug.Log("PV move " + pvNum + ": " + Move.GetMoveString(move));
                }
            } else if (Input.GetKeyDown(KeyCode.B)) {
                UnmakeMove();
            }
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
                            //mainBoard.pvTable.StorePvMove(moveToMake);
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
            Coordinates fromCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.FromSquare(move.move)));
            Coordinates toCoordinates =
                BoardSquares.CoordFromIndex(Board.Sq64(Move.ToSquare(move.move)));
            boardUi.SetHighlightColor(fromCoordinates);
            boardUi.SetHighlightColor(toCoordinates);
            boardUi.UpdateBoard(mainBoard);
            currentState = InputState.None;
            lastPlayedMove = move;
            ChangeTurn();
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
            if (changePlayer) currentPlayer = currentPlayer == PlayerType.Human ? PlayerType.AI : PlayerType.Human;
            Debug.Log("Turn changed.");
            //if (Search.IsRepeated(mainBoard)) Debug.Log("Position repeated");
        }

        private void AISearchAndMakeMove() {
            SearchInfo searchParameters = new SearchInfo(depth: 6);
            Search.SearchPosition(mainBoard, searchParameters);
            Move bestMove = mainBoard.pvArray[0];
            MakeMove(bestMove);
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