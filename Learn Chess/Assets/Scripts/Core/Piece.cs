namespace Core
{
    public static class Piece
    {
        
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

        public static bool IsWhite(int piece)
        {
            return (piece < 7) && (piece != 0);
        }

        public static bool IsBlack(int piece)
        {
            return piece > 6;
        }

        public static int getColor(int piece)
        {
            if ((piece < 7) && (piece >= 0))
            {
                return White;
            }
            if (piece >= 7 && piece <= 12)
            {
                return Black;
            }
            return Both;
        }
        
        public static int AbsolutePieceType(int piece)
        {
            if (piece > 6)
            {
                return piece - 6;
            }
            return piece;
        }


        /*public static bool isPieceBig(int piece)
        {
            int pieceType = AbsolutePieceType(piece);
            return pieceType == Knight || pieceType == Bishop || pieceType == Rook || pieceType == Queen ||
                    pieceType == King;
        }

        public static bool isPieceMajor(int piece)
        {
            int pieceType = AbsolutePieceType(piece);
            return pieceType == Rook || pieceType == Queen || pieceType == King;
        }

        public static bool isPieceMinor(int piece)
        {
            int pieceType = AbsolutePieceType(piece);
            return pieceType == Knight || pieceType == Bishop;
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
        }*/
        public static bool[] PieceBig = new bool[13] { false, false, true, true, true, true, true, false, true, true, true, true, true };
        public static bool[] PieceMaj = new bool[13] { false, false, false, false, true, true, true, false, false, false, true, true, true };
        public static bool[] PieceMin = new bool[13] { false, false, true, true, false, false, false, false, true, true, false, false, false };
        public static int[] PieceVal = new int[13] { 0, 100, 325, 325, 550, 1000, 50000, 100, 325, 325, 550, 1000, 50000 };
        public static int[] PieceCol = new int[13] { Piece.Both, Piece.White, Piece.White, Piece.White, Piece.White, Piece.White, Piece.White,
            Piece.Black, Piece.Black, Piece.Black, Piece.Black, Piece.Black, Piece.Black };
    }
}
