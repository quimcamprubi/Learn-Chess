namespace Core
{
    public static class Piece
    {
        
        public const int None = 0;
        public const int Pawn = 1;
        public const int Knight = 2;
        public const int Bishop = 3;
        public const int Rook = 4;
        public const int Queen = 5;
        public const int King = 6;
        
        public const int White = 1;
        public const int Black = 2;
        
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

        public static int AbsolutePieceType(int piece)
        {
            if (piece > 6)
            {
                return piece - 6;
            }
            return piece;
        }
    }
}
