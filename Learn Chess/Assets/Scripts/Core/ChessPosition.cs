﻿using Utils;

namespace Core {
    public class ChessPosition {
        public int[] squares;
        public bool whiteToMove;
        public int castlingRights;
        public int enPassantSquare;
        public int plyCount;
        
        public ChessPosition() {
            squares = new int[120];
            castlingRights = 0;
            enPassantSquare = (int) Board.Squares120Enum.NO_SQ;
            plyCount = 0;
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++) {
                squares[i] = (int) Board.Squares120Enum.OFFBOARD;
            }
            for (int i = 0; i < Constants.NUM_SQUARES; i++) {
                squares[Board.Sq120(i)] = Piece.Empty;
            }
        }
        
    }
}
