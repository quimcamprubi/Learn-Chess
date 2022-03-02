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
            squares = new int[64];
        }
        
    }
}
