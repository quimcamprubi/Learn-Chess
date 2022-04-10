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
        
        private Camera cam;
        private int selectedRank;
        private int selectedFile;
        private int lastSelectedIndex = -1;
        private int playerSide = Board.White;
        private Move lastPlayedMove = null;

        public enum InputState {
            None,
            PieceSelected
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
            Stopwatch watch = Stopwatch.StartNew();
            //PerftTesting.PerftTest(mainBoard, 7);
            watch.Stop();
            Debug.Log("Elapsed time: " + watch.Elapsed);
            /*List<Move> moveList = MoveGenerator.GenerateAllMoves(mainBoard);
            StartCoroutine(TestMoveGenerator(moveList));*/
            currentPseudoLegalMoves = MoveGenerator.GenerateAllMoves(mainBoard);
        }

        public void SwitchSides() {
            if (playerSide == Board.White) {
                boardUi.SetPerspective(false);
                playerSide = Board.Black;
                boardUi.isBottomWhite = false;
            } else {
                boardUi.SetPerspective(true);
                playerSide = Board.White;
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
            string lastPlayedMoveString =
                lastPlayedMove == null ? "-" : BoardSquares.GetAlgebraicMove(lastPlayedMove.move);
            sideToPlayText.GetComponent<ShowText>().textValue = GetSideToPlayString(mainBoard.sideToPlay);
            enPassantText.GetComponent<ShowText>().textValue =
                "En Passant square: " + BoardSquares.SquareNameDictionary[mainBoard.enPassantSquare];
            castlingRightsText.GetComponent<ShowText>().textValue = GetCastlingRightsString(mainBoard.castlingRights);
            lastMoveText.GetComponent<ShowText>().textValue = "Last move: " + lastPlayedMoveString;
            if (currentPseudoLegalMoves != null && !GameIsPaused) HandleInput();
        }

        private void HandleInput() {
            Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0)) {
                if (currentState == InputState.None) {
                    AttemptSelectPiece(mousePosition);
                } else {
                    CheckSquareSelected(mousePosition);
                }
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
                            if (!Move.IsMovePromotion(moveToMake.move)) {
                                mainBoard.MakeMove(moveToMake);
                                Coordinates fromCoordinates =
                                    BoardSquares.CoordFromIndex(Board.Sq64(Move.FromSquare(moveToMake.move)));
                                Coordinates toCoordinates =
                                    BoardSquares.CoordFromIndex(Board.Sq64(index));
                                boardUi.SetHighlightColor(fromCoordinates);
                                boardUi.SetHighlightColor(toCoordinates);
                                boardUi.UpdateBoard(mainBoard);
                                currentState = InputState.None;
                                lastPlayedMove = moveToMake;
                                ChangeTurn();
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

        public void ChangeTurn() {
            currentPseudoLegalMoves = MoveGenerator.GenerateAllMoves(mainBoard);
            CheckEnding();
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
            if (currentLegalMoves.Count == 0) {
                if (mainBoard.IsKingInCheck()) {
                    Checkmate();
                } else {
                    Stalemate();
                } 
            }
        }

        private void Checkmate() {
            Debug.Log("Checkmate");
            // TODO END GAME MESSAGE
        }

        private void Stalemate() {
            Debug.Log("Stalemate");
            // TODO STALEMATE MESSAGE
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