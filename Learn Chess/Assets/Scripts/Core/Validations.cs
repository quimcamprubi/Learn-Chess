using Utils;

namespace Core {
    public static class Validations {
        public static bool IsSquareOnBoard(int square) { return Board.squareFile[square] != Board.OFFBOARD; }
        public static bool IsSideValid(int side) { return side == Board.White || side == Board.Black; }
        public static bool IsFileRankValid(int fileRank) { return fileRank >= 0 && fileRank <= Constants.NUM_FILES; }
        public static bool IsPieceValidOrEmpty(int piece) { return piece >= Piece.Empty && piece <= Piece.BlackKing; }
        public static bool IsPieceValid(int piece) { return piece >= Piece.WhitePawn && piece <= Piece.BlackKing; }
    }
}
