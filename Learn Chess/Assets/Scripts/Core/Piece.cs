namespace Core
{
    public static class Piece
    {
        // Each piece will be represented as Color (2 bits) | Type (3 bits)
        
        public const int None = 0;      // x000
        public const int Pawn = 1;      // x001
        public const int Knight = 2;    // x010
        public const int Bishop = 3;    // x011
        public const int Rook = 4;      // x100
        public const int Queen = 5;     // x101
        public const int King = 6;      // x110

        public const int White = 8;     // 1xxx
        public const int Black = 16;     // 0xxx
        
        private const int TypeMask = 0b00111;
        private const int BlackMask = 0b10000;
        private const int WhiteMask = 0b01000;
        private const int ColorMask = WhiteMask | BlackMask;
        
        public static bool IsColor(int piece, int color)
        {
            return (piece & ColorMask) == color;
        }

        public static bool IsWhite(int piece)
        {
            return (piece & WhiteMask) == WhiteMask;
        }

        public static bool IsBlack(int piece)
        {
            return (piece & BlackMask) == BlackMask;
        }
        
        public static int PieceType (int piece) {
            return piece & TypeMask;
        }
    }
}
