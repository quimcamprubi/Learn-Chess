using Utils;

namespace Core
{
    public class Board
    {
        public bool WhiteToMove; 
        public int[] squares;

        public void loadStartingPosition()
        {
            ChessPosition loadedPosition = FenDecoder.DecodePositionFromFen(Constants.startingFen);
            squares = loadedPosition.squares;
            WhiteToMove = loadedPosition.whiteToMove;
        }
    }
}
