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

        public static bool IsWhite(int piece) { return (piece < 7) && (piece != 0); }
        public static bool IsBlack(int piece) { return piece > 6; }
        public static int GetColor(int piece) {
            if ((piece < 7) && (piece >= 0)) {
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
                    return 350;
                case Bishop:
                    return 350;
                case Rook:
                    return 525;
                case Queen:
                    return 1000;
                case King:
                    return 40000;
                default:
                    return 0;
            }
        }
        public static bool IsPiecePawn(int piece) { return piece == WhitePawn || piece == BlackPawn; }
        public static bool IsPieceBishop(int piece) { return piece == WhiteBishop || piece == BlackBishop; }
        public static bool IsPieceRook(int piece) { return piece == WhiteRook || piece == BlackRook; }
        public static bool IsPieceQueen(int piece) { return piece == WhiteQueen || piece == BlackQueen; }

        public static bool[] PieceBig = new bool[13] { false, false, true, true, true, true, true, false, true, true, true, true, true };
        public static bool[] PieceMaj = new bool[13] { false, false, false, false, true, true, true, false, false, false, true, true, true };
        public static bool[] PieceMin = new bool[13] { false, false, true, true, false, false, false, false, true, true, false, false, false };
        public static int[] PieceVal = new int[13] { 0, 100, 325, 325, 550, 1000, 50000, 100, 325, 325, 550, 1000, 50000 };
        public static int[] PieceColor = new int[13] { Both, White, White, White, White, White, White,
            Black, Black, Black, Black, Black, Black };
        public static bool[] IsPieceKnight = new bool[13] { false, false, true, false, false, false, false, false, true, false, false, false, false };
        public static bool[] IsPieceKing = new bool[13] { false, false, false, false, false, false, true, false, false, false, false, false, true };
        public static bool[] IsPieceRookQueen = new bool[13] { false, false, false, false, true, true, false, false, false, false, true, true, false };
        public static bool[] IsPieceBishopQueen = new bool[13] { false, false, false, true, false, true, false, false, false, true, false, true, false };
    }
}
