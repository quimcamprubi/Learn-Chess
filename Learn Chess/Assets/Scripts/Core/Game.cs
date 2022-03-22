using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class Game : MonoBehaviour
    {
        public BoardUI boardUi;
        private GameObject sideToPlayText;
        private GameObject enPassantText;
        private GameObject castlingRightsText;
        private GameObject positionKeyText;
        
        public enum InputState
        {
            None,
            DraggingPiece
        }

        private Board mainBoard;
        private InputState inputState;

        private void Start()
        {
            sideToPlayText = GameObject.Find("SideToPlayText");
            enPassantText = GameObject.Find("EnPassantText");
            castlingRightsText = GameObject.Find("CastlingRightsText");
            positionKeyText = GameObject.Find("PositionKeyText");
            boardUi = FindObjectOfType<BoardUI>();
            mainBoard = new Board();
            InitializeGame();
        }

        private void Update()
        {
            sideToPlayText.GetComponent<ShowText>().textValue = getSideToPlayString(mainBoard.sideToPlay);
            enPassantText.GetComponent<ShowText>().textValue =
                "En Passant square: " + mainBoard.enPassantSquare;
            castlingRightsText.GetComponent<ShowText>().textValue = getCastlingRightsString(mainBoard.castlingRights);
            positionKeyText.GetComponent<ShowText>().textValue = "Position Key: " + mainBoard.positionKey;
        }

        private void InitializeGame()
        {
            mainBoard.loadStartingPosition();
            boardUi.UpdateBoard(mainBoard);
        }

        private string getSideToPlayString(int sideToPlay)
        {
            return sideToPlay == Piece.White ? "Side to play: White" : "Side to play: Black";
        }

        private string getCastlingRightsString(int castlingRights)
        {
            string returnString = "Castling rights: ";
            returnString += (castlingRights & (int) CastlingRights.WKCA) != 0 ? "K" : "-";
            returnString += (castlingRights & (int) CastlingRights.WQCA) != 0 ? "Q" : "-";
            returnString += (castlingRights & (int) CastlingRights.BKCA) != 0 ? "k" : "-";
            returnString += (castlingRights & (int) CastlingRights.BQCA) != 0 ? "q" : "-";
            return returnString;
        }
    }
}