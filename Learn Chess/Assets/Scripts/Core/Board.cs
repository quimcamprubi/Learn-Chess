using System;
using Utils;

namespace Core
{
    public class Board
    {
        
        public int[] squares;
        
        private int[] board64 = new int[Constants.NUM_SQUARES];
        private int[] board120 = new int[Constants.NUM_SQUARES_EXT];
        private UInt64[] pawns = new UInt64[Constants.NUM_PIECE_VARIANTS];
        private int[] kingSquares = new int[Constants.NUM_KINGS];

        private int sideToPlay;
        private int enPassantSquareInd;
        private int fiftyMoveCounter;
        private int ply;
        private int histPly;

        private UInt64 positionKey;
        
        private int[] pieceNumbers = new int[Constants.TOTAL_DIFF_PIECES];
        private int[] bigPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[] majorPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[] minorPieces = new int[Constants.PIECE_TYPE_VARIANTS];

        public static int NoPlayer = 0;
        public static int White = 1;
        public static int Black = 2;

        public void loadStartingPosition()
        {
            ChessPosition loadedPosition = FenDecoder.DecodePositionFromFen(Constants.startingFen);
            squares = loadedPosition.squares;
        }
    }
}
