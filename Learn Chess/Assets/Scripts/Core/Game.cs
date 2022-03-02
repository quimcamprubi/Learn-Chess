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

        private Board primaryBoard;

        private void Start()
        {
            boardUi = FindObjectOfType<BoardUI>();
            primaryBoard = new Board();
            InitializeGame();
        }

        private void InitializeGame()
        {
            primaryBoard.loadStartingPosition();
            boardUi.UpdateBoard(primaryBoard);
        }
    }
}