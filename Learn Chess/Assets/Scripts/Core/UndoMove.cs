using System;

namespace Core {
    public class UndoMove {
        public int move;
        public int castlingRights;
        public int enPassant;
        public int fiftyMove;
        public ulong positionKey;

        public UndoMove() {
            move = 0;
            castlingRights = 0;
            enPassant = (int) Board.Squares120Enum.NO_SQ;
            fiftyMove = 0;
            positionKey = 0;
        }
        
        
    }
}
