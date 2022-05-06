using System.Diagnostics;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Core {
    public static class PerftTesting {
        public static long LeafNodes = 0;
        public static void RecursivePerft(Board board, int depth) {
            Debug.Assert(board.CheckBoard());
            if (depth == 0) {
                LeafNodes++;
                return;
            }
            Move[] moveList = MoveGenerator.GenerateAllMoves(board).ToArray();
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
            Debug.Assert(board.CheckBoard());
            LeafNodes = 0;
            Debug.Log("Starting Perft test to depth = " + depth);
            Stopwatch watch = Stopwatch.StartNew();
            Move[] moveList = MoveGenerator.GenerateAllMoves(board).ToArray();
            for (int i = 0; i < moveList.Length; i++) {
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
            watch.Stop();
            Debug.Log("Perft test complete: " + LeafNodes + " visited.");
            Debug.Log("Elapsed time: " + watch.ElapsedMilliseconds);
            return nodes;
        }
    }
}
