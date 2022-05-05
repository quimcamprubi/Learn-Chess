using System.Collections.Generic;

namespace Core {
    public static class Piece {
        
        public const int Empty = 0;
        public const int Pawn = 1;
        public const int Knight = 2;
        public const int Bishop = 3;
        public const int Rook = 4;
        public const int Queen = 5;
        public const int King = 6;
        
        public const int White = 0;
        public const int Black = 1;
        public const int Both = 2;
        
        public const int WhitePawn = 1;
        public const int WhiteKnight = 2;
        public const int WhiteBishop = 3;
        public const int WhiteRook = 4;
        public const int WhiteQueen = 5;
        public const int WhiteKing = 6;
        public const int BlackPawn = 7;
        public const int BlackKnight = 8;
        public const int BlackBishop = 9;
        public const int BlackRook = 10;
        public const int BlackQueen = 11;
        public const int BlackKing = 12;

        public static Dictionary<int, string> PieceStrings = new Dictionary<int, string>() {
            [Empty] = "Empty",
            [WhitePawn] = "White Pawn",
            [WhiteKnight] = "White Knight",
            [WhiteBishop] = "White Bishop",
            [WhiteRook] = "White Rook",
            [WhiteQueen] = "White Queen",
            [WhiteKing] = "White King",
            [BlackPawn] = "Black Pawn",
            [BlackKnight] = "Black Knight",
            [BlackBishop] = "Black Bishop",
            [BlackRook] = "Black Rook",
            [BlackQueen] = "Black Queen",
            [BlackKing] = "Black King"
        };
        
        public static bool IsWhite(int piece) { return (piece < 7) && (piece != 0); }
        public static bool IsBlack(int piece) { return piece > 6; }
        public static int GetColor(int piece) {
            if ((piece < 7) && (piece > 0)) {
                return White;
            }
            if (piece >= 7 && piece <= 12) {
                return Black;
            }
            return Both;
        }
        public static int AbsolutePieceType(int piece) {
            if (piece > 6) {
                return piece - 6;
            }
            return piece;
        }

        public static int getPieceValue(int piece)
        {
            switch (AbsolutePieceType(piece))
            {
                case Pawn:
                    return 100;
                case Knight:
                    return 325;
                case Bishop:
                    return 325;
                case Rook:
                    return 550;
                case Queen:
                    return 1000;
                case King:
                    return 50000;
                default:
                    return 0;
            }
        }
        public static bool IsPiecePawn(int piece) { return piece == WhitePawn || piece == BlackPawn; }
        public static bool IsPieceBishop(int piece) { return piece == WhiteBishop || piece == BlackBishop; }
        public static bool IsPieceRook(int piece) { return piece == WhiteRook || piece == BlackRook; }
        public static bool IsPieceQueen(int piece) { return piece == WhiteQueen || piece == BlackQueen; }

        public static readonly bool[] PieceBig = { false, false, true, true, true, true, true, false, true, true, true, true, true };
        public static readonly bool[] PieceMaj = { false, false, false, false, true, true, true, false, false, false, true, true, true };
        public static readonly bool[] PieceMin = { false, false, true, true, false, false, false, false, true, true, false, false, false };
        public static readonly int[] PieceVal = { 0, 100, 325, 325, 550, 1000, 50000, 100, 325, 325, 550, 1000, 50000 };
        public static readonly int[] PieceColor = { Both, White, White, White, White, White, White,
            Black, Black, Black, Black, Black, Black };
        public static readonly bool[] IsPieceKnight = { false, false, true, false, false, false, false, false, true, false, false, false, false };
        public static readonly bool[] IsPieceKing = { false, false, false, false, false, false, true, false, false, false, false, false, true };
        public static readonly bool[] IsPieceRookQueen = { false, false, false, false, true, true, false, false, false, false, true, true, false };
        public static readonly bool[] IsPieceBishopQueen = { false, false, false, true, false, true, false, false, false, true, false, true, false };
        public static readonly bool[] IsPieceSliding = { false, false, false, true, true, true, false, false, false, true, true, true, false };
        public static readonly int[] VictimScores = { 0, 100, 200, 300, 400, 500, 600, 100, 200, 300, 400, 500, 600 };
    }
}
