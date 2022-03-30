using System.Collections.Generic;

namespace Core {
    public static class PerftTesting {
        public static int PerftCount(Board board, int depth) {
            int nodes = 0;
            board.CheckBoard();
            if (depth == 0) {
                return 1;
            }
            List<Move> moveList = MoveGenerator.GenerateAllMoves(board);
            foreach (Move move in moveList) {
                if (!board.MakeMove(move)) { // If move is illegal, do not explore any further
                    continue;
                }
                nodes += PerftCount(board, depth - 1);
                board.UnmakeMove();
            }
            return nodes;
        }
        
        public static int PerftTest(Board board, int depth) {
            int nodes = 0;
            board.CheckBoard();
            board.PrintGameBoard();
            
            if (depth == 0) {
                return 1;
            }
            List<Move> moveList = MoveGenerator.GenerateAllMoves(board);
            foreach (Move move in moveList) {
                if (!board.MakeMove(move)) { // If move is illegal, do not explore any further
                    continue;
                }
                nodes += PerftCount(board, depth - 1);
                board.UnmakeMove();
            }
            return nodes;
        }
    }
}
