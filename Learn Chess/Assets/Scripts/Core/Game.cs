using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Core
{
    public class Game : MonoBehaviour
    {
        public enum PlayerType { Human, AI }
        public PlayerType whitePlayerType;
        public PlayerType blackPlayerType;
        public BoardUI boardUi;

        public enum InputState
        {
            None,
            DraggingPiece
        }

        private Board mainBoard;
        private InputState inputState;

        private void Start()
        {
            boardUi = FindObjectOfType<BoardUI>();
            mainBoard = new Board();
            InitializeGame();
        }

        private void InitializeGame()
        {
            mainBoard.loadStartingPosition();
            boardUi.UpdateBoard(mainBoard);
        }
    }
}