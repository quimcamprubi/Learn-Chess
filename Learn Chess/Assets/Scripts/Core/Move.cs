namespace Core {
    public class Move {
        public int move;
        public int score;
        public static int FromSquare(int move) { return move & 0x3F; }
        public static int ToSquare(int move) { return (move >> 7) & 0x3F; }
        public static int CapturedPiece(int move) { return (move >> 14) & 0xF; }
        public static int PromotedPiece(int move) { return (move >> 20) & 0xF; }
        public static bool IsEnPassantCapture(int move) { return (move & 0x40000) != 0; }
        public static bool IsPawnStartMove(int move) { return (move & 0x80000) != 0; }
        public static bool IsCastlingMove(int move) { return (move & 0x1000000) != 0; }
        public static bool IsMoveCapture(int move) { return (move & 0x7C000) != 0; }
        public static bool IsMovePromotion(int move) { return (move & 0xF00000) != 0; }
    }
    
    /* ----- move variable -----
    Since the maximum index a move can be From or To is 98, we need 7 bits to represent the square indices.
    Pieces captured can go up to 13, meaning we need 4 bits to represent the captured piece. 
    Resulting masks:
    
    0000 0000 0000 0000 0000 0111 1111 -> From Square                       : 0x3F
    0000 0000 0000 0011 1111 1000 0000 -> To Square                         : >> 7, 0x3F   
    0000 0000 0011 1100 0000 0000 0000 -> Captured piece                    : >> 14, 0xF
    0000 0000 0100 0000 0000 0000 0000 -> En Passant capture (yes or no)    : 0x40000
    0000 0000 1000 0000 0000 0000 0000 -> Pawn Start (yes or no)            : 0x80000
    0000 1111 0000 0000 0000 0000 0000 -> Promoted piece                    : >> 20, 0xF
    0001 0000 0000 0000 0000 0000 0000 -> Castling move (yes or no)         : 0x1000000
    
    Is move a capture (any)?:
    0000 0000 0011 1100 0000 0000 0000 -> Captured piece
    0000 0000 0100 0000 0000 0000 0000 -> En Passant capture
    0000 0000 0111 1100 0000 0000 0000 -> Any sort of capture               : 0x7C000
    */
    
}
