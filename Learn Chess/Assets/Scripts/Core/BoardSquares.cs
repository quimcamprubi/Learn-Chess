using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using Utils;

namespace Core {
    public static class BoardSquares {
        public static string RANKS = "12345678";
        public static string files = "abcdefgh";
        public static Dictionary<int, string> SquareNameDictionary = new Dictionary<int, string>() {
            [-1] = "None", [21] = "A1", [22] = "B1", [23] = "C1", [24] = "D1", [25] = "E1", [26] = "F1", [27] = "G1", [28] = "H1",
            [31] = "A2", [32] = "B2", [33] = "C2", [34] = "D2", [35] = "E2", [36] = "F2", [37] = "G2", [38] = "H2",
            [41] = "A3", [42] = "B3", [43] = "C3", [44] = "D3", [45] = "E3", [46] = "F3", [47] = "G3", [48] = "H3", 
            [51] = "A4", [52] = "B4", [53] = "C4", [54] = "D4", [55] = "E4", [56] = "F4", [57] = "G4", [58] = "H4",
            [61] = "A5", [62] = "B5", [63] = "C5", [64] = "D5", [65] = "E5", [66] = "F5", [67] = "G5", [68] = "H5",
            [71] = "A6", [72] = "B6", [73] = "C6", [74] = "D6", [75] = "E6", [76] = "F6", [77] = "G6", [78] = "H6",
            [81] = "A7", [82] = "B7", [83] = "C7", [84] = "D7", [85] = "E7", [86] = "F7", [87] = "G7", [88] = "H7",
            [91] = "A8", [92] = "B8", [93] = "C8", [94] = "D8", [95] = "E8", [96] = "F8", [97] = "G8", [98] = "H8",
        };
        
        public static int[] Mirror64 = new int[64] {
            56	,	57	,	58	,	59	,	60	,	61	,	62	,	63	,
            48	,	49	,	50	,	51	,	52	,	53	,	54	,	55	,
            40	,	41	,	42	,	43	,	44	,	45	,	46	,	47	,
            32	,	33	,	34	,	35	,	36	,	37	,	38	,	39	,
            24	,	25	,	26	,	27	,	28	,	29	,	30	,	31	,
            16	,	17	,	18	,	19	,	20	,	21	,	22	,	23	,
            8	,	9	,	10	,	11	,	12	,	13	,	14	,	15	,
            0	,	1	,	2	,	3	,	4	,	5	,	6	,	7
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
        
        public static string GetAlgebraicSquare(int square) {
            if (SquareNameDictionary.ContainsKey(square)) {
                return SquareNameDictionary[Move.FromSquare(square)];
            }
            return "Invalid square";
        }

        public static string GetAlgebraicMove(int move) {
            if (SquareNameDictionary.ContainsKey(Move.FromSquare(move))) {
                string returnString = "";
                returnString += SquareNameDictionary[Move.FromSquare(move)];
                returnString += SquareNameDictionary[Move.ToSquare(move)];
                int promoted = Move.PromotedPiece(move);
                if (promoted != 0) {
                    char promotedPiece = 'q';
                    if (Board.IsPieceKnight(promoted)) {
                        promotedPiece = 'n';
                    } else if (Piece.IsPieceRook(promoted)) {
                        promotedPiece = 'r';
                    } else if (Piece.IsPieceBishop(promoted)) {
                        promotedPiece = 'b';
                    }
                    returnString += promotedPiece;
                }
                return returnString;
            }
            return "Invalid move";
        }
    }
}
