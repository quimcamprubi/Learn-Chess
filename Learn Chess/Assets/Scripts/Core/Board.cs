using System;
using Utils;
using UnityEngine;
using System.Collections;

namespace Core
{
    public class Board
    {
        
        public int[] squares;
        
        private int[] sq64ToSq120 = new int[Constants.NUM_SQUARES];
        private int[] sq120ToSq64 = new int[Constants.NUM_SQUARES_EXT];
        private UInt64[] pawns = new UInt64[Constants.NUM_PIECE_VARIANTS];
        private int[] kingSquares = new int[Constants.NUM_KINGS];

        private int sideToPlay;
        private int enPassantSquareInd;
        private int fiftyMoveCounter;
        private int ply;
        private int histPly;
        private int castlingRights;
        private Move[] gameHist = new Move[Constants.MAX_GAME_MOVES];

        private UInt64 positionKey;
        
        private int[] pieceNumbers = new int[Constants.TOTAL_DIFF_PIECES];
        private int[] bigPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[] majorPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[] minorPieces = new int[Constants.PIECE_TYPE_VARIANTS];

        public static int NoPlayer = 0;
        public static int White = 1;
        public static int Black = 2;

        public enum SquaresEnum
        {
            A1 = 21, B1, C1, D1, E1, F1, G1, H1,
            A2 = 31, B2, C2, D2, E2, F2, G2, H2,
            A3 = 41, B3, C3, D3, E3, F3, G3, H3,
            A4 = 51, B4, C4, D4, E4, F4, G4, H4,
            A5 = 61, B5, C5, D5, E5, F5, G5, H5,
            A6 = 71, B6, C6, D6, E6, F6, G6, H6,
            A7 = 81, B7, C7, D7, E7, F7, G7, H7,
            A8 = 91, B8, C8, D8, E8, F8, G8, H8, NO_SQ
        }

        public void loadStartingPosition()
        {
            ChessPosition loadedPosition = FenDecoder.DecodePositionFromFen(Constants.startingFen);
            squares = loadedPosition.squares;
            initBoards();
        }

        private void initBoards()
        {
            int square = (int) SquaresEnum.A1;
            int square64 = 0;
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++)
            {
                sq120ToSq64[i] = 65;
            }

            for (int i = 0; i < Constants.NUM_SQUARES; i++)
            {
                sq64ToSq120[i] = 120;
            }

            for (int rank = 0; rank < Constants.NUM_RANKS; rank++)
            {
                for (int file = 0; file < Constants.NUM_FILES; file++)
                {
                    square = frTo120Sq(file, rank);
                    sq64ToSq120[square64] = square;
                    sq120ToSq64[square] = square64;
                    square64++;
                }
            }
        }

        public static int frTo120Sq(int file, int rank)
        {
            return (21 + file + rank * 10);
        }
    }
}
