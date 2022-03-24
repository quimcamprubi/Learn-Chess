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

        public enum InputState {
            None,
            DraggingPiece
        }

        private Board mainBoard;
        private InputState inputState;

        private void Start() {
            sideToPlayText = GameObject.Find("SideToPlayText");
            enPassantText = GameObject.Find("EnPassantText");
            castlingRightsText = GameObject.Find("CastlingRightsText");
            positionKeyText = GameObject.Find("PositionKeyText");
            boardUi = FindObjectOfType<BoardUI>();
            mainBoard = new Board();
            InitializeGame();
        }

        private void Update() {
            sideToPlayText.GetComponent<ShowText>().textValue = GetSideToPlayString(mainBoard.sideToPlay);
            enPassantText.GetComponent<ShowText>().textValue =
                "En Passant square: " + mainBoard.enPassantSquare;
            castlingRightsText.GetComponent<ShowText>().textValue = GetCastlingRightsString(mainBoard.castlingRights);
            positionKeyText.GetComponent<ShowText>().textValue = "Position Key: " + mainBoard.positionKey;
            
        }

        private void InitializeGame() {
            mainBoard.LoadStartingPosition();
            boardUi.UpdateBoard(mainBoard);
            mainBoard.LoadPosition(FenDecoder.DecodePositionFromFen(Constants.fen4));
            /*mainBoard.ShowSquaresAttackedBySide(Board.White);
            mainBoard.ShowSquaresAttackedBySide(Board.Black);*/
            boardUi.UpdateBoard(mainBoard);
            int from = 6;
            int to = 12;
            int captured = Piece.WhiteRook;
            int promoted = Piece.BlackRook;
            int move = Move.CreateMove(from, to, captured, promoted);
            Move.PrintMoveData(move);
            move |= Move.MaskPawnStart;
            Move.PrintMoveData(move);
            move = Move.CreateMove(from, to, captured, promoted, enPassantCapture: true, pawnStart: false,
                castlingMove: true);
            Move.PrintMoveData(move);
        }

        private string GetSideToPlayString(int sideToPlay) {
            return sideToPlay == Piece.White ? "Side to play: White" : "Side to play: Black";
        }

        private string GetCastlingRightsString(int castlingRights) {
            string returnString = "Castling rights: ";
            returnString += (castlingRights & (int) CastlingRights.WKCA) != 0 ? "K" : "-";
            returnString += (castlingRights & (int) CastlingRights.WQCA) != 0 ? "Q" : "-";
            returnString += (castlingRights & (int) CastlingRights.BKCA) != 0 ? "k" : "-";
            returnString += (castlingRights & (int) CastlingRights.BQCA) != 0 ? "q" : "-";
            return returnString;
        }
    }
}