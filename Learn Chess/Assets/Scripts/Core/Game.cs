using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Core {
    public class Game : MonoBehaviour {
        public BoardUI boardUi;
        private GameObject sideToPlayText;
        private GameObject enPassantText;
        private GameObject castlingRightsText;
        private GameObject positionKeyText;
        private Camera cam;
        private int selectedRank;
        private int selectedFile;
        public List<Move> currentPseudoLegalMoves;
        public List<Move> currentAvailableMoves;
        private int lastSelectedIndex = -1;
        
        public enum InputState {
            None,
            PieceSelected
        }

        private Board mainBoard;
        private InputState currentState;

        private void Start() {
            sideToPlayText = GameObject.Find("SideToPlayText");
            enPassantText = GameObject.Find("EnPassantText");
            castlingRightsText = GameObject.Find("CastlingRightsText");
            positionKeyText = GameObject.Find("PositionKeyText");
            boardUi = FindObjectOfType<BoardUI>();
            mainBoard = new Board();
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
            boardUi.UpdateBoard(mainBoard);
            /*List<Move> moveList = MoveGenerator.GenerateAllMoves(mainBoard);
            StartCoroutine(TestMoveGenerator(moveList));*/
            currentPseudoLegalMoves = MoveGenerator.GenerateAllMoves(mainBoard);
            

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
            sideToPlayText.GetComponent<ShowText>().textValue = GetSideToPlayString(mainBoard.sideToPlay);
            enPassantText.GetComponent<ShowText>().textValue =
                "En Passant square: " + mainBoard.enPassantSquare;
            castlingRightsText.GetComponent<ShowText>().textValue = GetCastlingRightsString(mainBoard.castlingRights);
            positionKeyText.GetComponent<ShowText>().textValue = "Position Key: " + mainBoard.positionKey;
            if (currentPseudoLegalMoves != null) HandleInput();
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
                            mainBoard.MakeMove(currentAvailableMoves[moveIndex]);
                            boardUi.UpdateBoard(mainBoard);
                            currentState = InputState.None;
                            ChangeTurn();
                        }
                    } else {
                        lastSelectedIndex = index;
                        currentAvailableMoves = mainBoard.GetLegalMovesForSquare(index, currentPseudoLegalMoves);
                        boardUi.HighlightLegalMoves(currentAvailableMoves);
                        currentState = InputState.PieceSelected;
                    }
                }
            }
        }

        private void AttemptSelectPiece(Vector2 mousePosition) {
            if (boardUi.TryGetSquareUnderMouse(mousePosition, out selectedFile, out selectedRank)) {
                int index = Board.FrTo120Sq(selectedFile, selectedRank);
                int color = Piece.GetColor(mainBoard.squares[index]);
                //if (color == mainBoard.sideToPlay) {
                    lastSelectedIndex = index;
                    currentAvailableMoves = mainBoard.GetLegalMovesForSquare(index, currentPseudoLegalMoves);
                    boardUi.HighlightLegalMoves(currentAvailableMoves);
                    currentState = InputState.PieceSelected;
                //}
            }
        }

        public void ChangeTurn() {
            currentPseudoLegalMoves = MoveGenerator.GenerateAllMoves(mainBoard);
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
        
        
    }
}