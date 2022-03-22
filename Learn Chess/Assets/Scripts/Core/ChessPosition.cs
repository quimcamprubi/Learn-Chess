using Utils;

namespace Core
{
    public class ChessPosition
    {
        public int[] squares;
        public bool whiteToMove;
        public bool whiteKingSideCastling;
        public bool whiteQueenSideCastling;
        public bool blackKingSideCastling;
        public bool blackQueenSideCastling;
        public int epFile;
        public int plyCount;
        
        public ChessPosition()
        {
            squares = new int[120];

            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++)
            {
                squares[i] = (int) Board.Squares120Enum.OFFBOARD;
            }

            for (int i = 0; i < Constants.NUM_SQUARES; i++)
            {
                squares[Board.sq120(i)] = Piece.Empty;
            }
        }
        
    }
}
