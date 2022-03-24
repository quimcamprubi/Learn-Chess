using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using Utils;

namespace Core {
    public static class BoardSquares {
        public const string RANKS = "12345678";
        public const string files = "abcdefgh";

        public static int RankIndex (int squareIndex) {
            return squareIndex >> 3;
        }

        // File (0 to 7) of square 
        public static int FileIndex (int squareIndex) {
            return squareIndex & 0b000111;
        }

        public static int IndexFromCoord (int fileIndex, int rankIndex) {
            return rankIndex * Constants.NUM_FILES + fileIndex;
        }

        public static int IndexFromCoord (Coordinates coordinates) {
            return IndexFromCoord (coordinates.fileIndex, coordinates.rankIndex);
        }

        public static Coordinates CoordFromIndex (int squareIndex) {
            return new Coordinates (FileIndex (squareIndex), RankIndex (squareIndex));
        }

        public static bool LightSquare (int fileIndex, int rankIndex) {
            return (fileIndex + rankIndex) % 2 != 0;
        }

        public static string SquareNameFromCoordinate (int fileIndex, int rankIndex) {
            return files[fileIndex] + "" + (rankIndex + 1);
        }

        public static string SquareNameFromIndex (int squareIndex) {
            return SquareNameFromCoordinate (CoordFromIndex (squareIndex));
        }

        public static string SquareNameFromCoordinate (Coordinates coord) {
            return SquareNameFromCoordinate (coord.fileIndex, coord.rankIndex);
        }
    }
}
