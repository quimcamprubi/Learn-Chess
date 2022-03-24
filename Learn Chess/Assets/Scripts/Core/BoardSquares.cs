using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using Utils;

namespace Core {
    public static class BoardSquares {
        public const string RANKS = "12345678";
        public const string files = "abcdefgh";
        public static Dictionary<int, string> SquareNameDictionary = new Dictionary<int, string>() {
            [21] = "a1", [22] = "b1", [23] = "c1", [24] = "d1", [25] = "e1", [26] = "f1", [27] = "g1", [28] = "h1",
            [31] = "a2", [32] = "b2", [33] = "c2", [34] = "d2", [35] = "e2", [36] = "f2", [37] = "g2", [38] = "h2",
            [41] = "a3", [42] = "b3", [43] = "c3", [44] = "d3", [45] = "e3", [46] = "f3", [47] = "g3", [48] = "h3", 
            [51] = "a4", [52] = "b4", [53] = "c4", [54] = "d4", [55] = "e4", [56] = "f4", [57] = "g4", [58] = "h4",
            [61] = "a5", [62] = "b5", [63] = "c5", [64] = "d5", [65] = "e5", [66] = "f5", [67] = "g5", [68] = "h5",
            [71] = "a6", [72] = "b6", [73] = "c6", [74] = "d6", [75] = "e6", [76] = "f6", [77] = "g6", [78] = "h6",
            [81] = "a7", [82] = "b7", [83] = "c7", [84] = "d7", [85] = "e7", [86] = "f7", [87] = "g7", [88] = "h7",
            [91] = "a8", [92] = "b8", [93] = "c8", [94] = "d8", [95] = "e8", [96] = "f8", [97] = "g8", [98] = "h8",
        };

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
