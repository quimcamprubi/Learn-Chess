using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core {
    public static class PerftTesting {
        public static long LeafNodes = 0;
        public static void RecursivePerft(Board board, int depth) {
            Assert.IsTrue(board.CheckBoard());
            if (depth == 0) {
                LeafNodes++;
                return;
            }
            List<Move> moveList = MoveGenerator.GenerateAllMoves(board);
            foreach (Move move in moveList) {
                if (!board.MakeMove(move)) { // If move is illegal, do not explore any further
                    continue;
                }
                RecursivePerft(board, depth - 1);
                board.UnmakeMove();
            }
        }
        
        public static int PerftTest(Board board, int depth) {
            int nodes = 0;
            Assert.IsTrue(board.CheckBoard());
            board.PrintGameBoard();
            LeafNodes = 0;
            Debug.Log("Starting Perft test to depth = " + depth);
            List<Move> moveList = MoveGenerator.GenerateAllMoves(board);
            for (int i = 0; i < moveList.Count; i++) {
                Move move = moveList[i];
                if (!board.MakeMove(move)) { // If move is illegal, do not explore branch
                    continue;
                }
                long cumulativeNodes = LeafNodes; 
                RecursivePerft(board, depth - 1);
                board.UnmakeMove();
                long oldNodes = LeafNodes - cumulativeNodes;
                Debug.Log("Move " + (i + 1) + " : " + BoardSquares.GetAlgebraicMove(move.move) +  " : " + oldNodes);
            }
            Debug.Log("Perft test complete: " + LeafNodes + " visited.");
            return nodes;
        }
    }
}
