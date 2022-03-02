using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Utils;

namespace UI
{
    public class BoardUI : MonoBehaviour
    {
        public bool isWhitePlaying = true;
        public BoardColors boardColors;
        public PieceThemes pieceThemes;
        private MeshRenderer[, ] squareRenderers;
        private SpriteRenderer[, ] squarePieceRenderers;

        private void Awake()
        {
            GenerateBoard();
        }

        public void GenerateBoard()
        {
            Shader squareShader = Shader.Find("Unlit/Color");
            squareRenderers = new MeshRenderer[8, 8];
            squarePieceRenderers = new SpriteRenderer[8, 8];
            for (int rank = 0; rank < Constants.NUM_RANKS; rank++)
            {
                for (int file = 0; file < Constants.NUM_FILES; file++)
                {
                    Transform square = GameObject.CreatePrimitive (PrimitiveType.Quad).transform;
                    square.parent = transform;
                    square.name = BoardSquares.SquareNameFromCoordinate (file, rank);
                    square.position = PositionFromCoordinates (file, rank, -0.1f);
                    Material squareMaterial = new Material (squareShader);
                    squareRenderers[file, rank] = square.gameObject.GetComponent<MeshRenderer> ();
                    squareRenderers[file, rank].material = squareMaterial;
                    
                    SpriteRenderer pieceRenderer = new GameObject ("Piece").AddComponent<SpriteRenderer> ();
                    pieceRenderer.transform.parent = square;
                    pieceRenderer.transform.position = PositionFromCoordinates (file, rank, 1);
                    pieceRenderer.transform.localScale = Vector3.one * 100 / (2000 / 6f);
                    squarePieceRenderers[file, rank] = pieceRenderer;
                }
            }
            ResetSquareColors();
        }
        
        public Vector3 PositionFromCoordinates (int file, int rank, float depth = 0) {
            if (isWhitePlaying) {
                return new Vector3 (-3.5f + file, -3.5f + rank, depth);
            }
            return new Vector3 (-3.5f + 7 - file, 7 - rank - 3.5f, depth);
        }

        public void ResetSquareColors()
        {
            for (int rank = 0; rank < 8; rank++) {
                for (int file = 0; file < 8; file++) {
                    SetSquareColor(new Coordinates(file, rank), boardColors.lightColors.normal, boardColors.darkColors.normal);
                }
            }
        }

        void SetSquareColor(Coordinates square, Color lightColor, Color darkColor)
        {
            squareRenderers[square.fileIndex, square.rankIndex].material.color =
                square.IsLightSquare() ? lightColor : darkColor;
        }

        public void UpdateBoard(Board primaryBoard)
        {
            for (int rank = 0; rank < Constants.NUM_RANKS; rank++)
            {
                for (int file = 0; file < Constants.NUM_FILES; file++)
                {
                    squarePieceRenderers[file, rank].sprite =
                        pieceThemes.GetPieceSprite(primaryBoard.squares[rank * 8 + file]);
                    squarePieceRenderers[file, rank].transform.position = PositionFromCoordinates(file, rank, -0.1f);
                }
            }
        }
    }
}
