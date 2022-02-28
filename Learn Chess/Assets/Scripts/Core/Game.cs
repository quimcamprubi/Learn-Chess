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

        private void Start()
        {
            boardUi = FindObjectOfType<BoardUI>();
            InitializeGame();
        }

        private void InitializeGame()
        {
            
        }
    }
}