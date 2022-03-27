using System.Collections.Generic;

namespace Core {
    public static class Directions {
        public static readonly int[] KnightDirections = {-8, -19, -21, -12, 8, 19, 21, 12};
        public static readonly int[] BishopDirections = {-9, -11, 11, 9};
        public static readonly int[] RookDirections = {-1, -10, 1, 10};
        public static readonly int[] KingDirections = {-1, -10, 1, 10, -9, -11, 11, 9};
        public static readonly int PawnForward = 10; // Simple forward move
        public static readonly int PawnDoubleForward = 20; // Double forward move (Pawn Start)
        public static readonly int PawnCapLeft = 9; // Capture left
        public static readonly int PawnCapRight = 11; // Capture right

        public static Dictionary<int, List<int>> PieceDirections = new Dictionary<int, List<int>>() {
            [Piece.Empty] = new List<int> {},
            [Piece.WhitePawn] = new List<int> {},
            [Piece.WhiteKnight] = new List<int> {21, 12, -8, -19, -21, -12, 8, 19},
            [Piece.WhiteBishop] = new List<int> {11, -9, -11, 9},
            [Piece.WhiteRook] = new List<int> {10, 1, -10, -1},
            [Piece.WhiteQueen] = new List<int> {10, 1, -10, -1, 11, -9, -11, 9},
            [Piece.WhiteKing] = new List<int> {10, 1, -10, -1, 11, -9, -11, 9},
            [Piece.BlackPawn] = new List<int> {},
            [Piece.BlackKnight] = new List<int> {21, 12, -8, -19, -21, -12, 8, 19},
            [Piece.BlackBishop] = new List<int> {11, -9, -11, 9},
            [Piece.BlackRook] = new List<int> {10, 1, -10, -1},
            [Piece.BlackQueen] = new List<int> {10, 1, -10, -1, 11, -9, -11, 9},
            [Piece.BlackKing] = new List<int> {10, 1, -10, -1, 11, -9, -11, 9},
        };
        
        // DELETE IF UNNECESSARY
        /*public static int[,] AllPieceDirections = new int [13, 8] { // Used to run through all the pieces in a single loop in MoveGenerator
            { 0, 0, 0, 0, 0, 0, 0, 0 },             // Empty
            { 0, 0, 0, 0, 0, 0, 0, 0 },             // White Pawn - We use a different algorithm for pawn moves
            {-8, -19, -21, -12, 8, 19, 21, 12},     // White Knight
            {-9, -11, 11, 9, 0, 0, 0, 0},           // White Bishop
            {-1, -10, 1, 10, 0, 0, 0, 0},           // White Rook
            {-1, -10, 1, 10, -9, -11, 11, 9},       // White Queen
            {-1, -10, 1, 10, -9, -11, 11, 9},       // White King
            { 0, 0, 0, 0, 0, 0, 0, 0 },             // Black Pawn - We use a different algorithm for pawn moves
            {-8, -19, -21, -12, 8, 19, 21, 12},     // Black Knight
            {-9, -11, 11, 9, 0, 0, 0, 0},           // Black Bishop
            {-1, -10, 1, 10, 0, 0, 0, 0},           // Black Rook
            {-1, -10, 1, 10, -9, -11, 11, 9},       // Black Queen
            {-1, -10, 1, 10, -9, -11, 11, 9}        // Black King
        };
        public static int[] AllPiecesNumDirections = new int[13] {
            0, 0, 8, 4, 4, 8, 8, 0, 8, 4, 4, 8, 8   // Used to know how much we should iterate through the AllPiecesDirections for every piece.
        };*/
    }
}
