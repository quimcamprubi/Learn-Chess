using System.Diagnostics;
using System.Linq;
using UnityEngine.Assertions;
using Utils;
using Debug = UnityEngine.Debug;

namespace Core {
    public class SearchInfo {
        public int startTime;
        public int stopTime;
        public int depth;
        public int depthSet;
        public int timeSet;
        public int movesToGo;
        public long nodes;
        public bool quit;
        public bool stopped;
        public bool infinite;

        public SearchInfo(int depth) {
            this.depth = depth;
        }
        
        public void ClearSearchData() {
            startTime = (int) Stopwatch.GetTimestamp();
            stopped = false;
            nodes = 0;
        }
    }
    
    public static class Search {
        public static int Infinite = 30000;
        public static int Mate = 29000;
        
        public static void CheckFinish() {
            // Check if Search has been running for too long
        }
        
        public static bool IsRepeated(Board board) {
            for (int i = board.histPly - board.fiftyMoveCounter; i < board.histPly; i++) {
                if (board.gameHist[i].positionKey == board.positionKey) return true;
            }
            return false;
        }

        public static void ClearForSearch(Board board, SearchInfo searchInfo) {
            board.ClearSearchData();
            searchInfo.ClearSearchData();
        } 
        
        public static int QuiescenceSearch(int alpha, int beta, Board board, SearchInfo searchInfo) { // Same as AlphaBeta, but with additional search after capture
            //TODO
            return 0;
        }
        
        public static int RecursiveAlphaBeta(int alpha, int beta, int depth, Board board, SearchInfo searchInfo, bool nullMove = false) {
            //Assert.IsTrue(board.CheckBoard(), "Checkboard failed at RecursiveAlphaBeta");
            if (depth == 0) {
                searchInfo.nodes++;
                return Evaluate.EvaluatePosition(board);
            }
            
            searchInfo.nodes++;
            if (IsRepeated(board) || board.fiftyMoveCounter >= 100) {
                return 0;
            }
            
            if (board.ply > Constants.MAX_DEPTH - 1) {
                return Evaluate.EvaluatePosition(board);
            }
            
            int legalMoves = 0;
            int oldAlpha = alpha;
            Move bestMove = new Move(0, -Infinite);
            Move[] movesList = MoveGenerator.GenerateAllMoves(board).ToArray();
            foreach (Move move in movesList) {
                if (!board.MakeMove(move)) continue;
                legalMoves++;
                int score = -RecursiveAlphaBeta(-beta, -alpha, depth - 1, board, searchInfo, true); // Negative because it is a Negamax implementation
                board.UnmakeMove();
                if (score > alpha) {
                    if (score >= beta) {
                        return beta;
                    }
                    alpha = score;
                    bestMove = move;
                }
            }

            if (legalMoves == 0) {
                if (board.IsSquareAttacked(board.kingSquares[board.sideToPlay], board.sideToPlay ^ 1)) {
                    return -Mate + board.ply; // Checkmate, taking into account how far it is
                }
                return 0; // Stalemate
            }
            
            if (alpha != oldAlpha) {
                board.pvTable.StorePvMove(bestMove);
            }
            
            return alpha;
        }
        
        public static void SearchPosition(Board board, SearchInfo searchInfo) { // Iterative deepening function
            //Move bestMove = new Move(Board.None, -Infinite);
            ClearForSearch(board, searchInfo);
            for (int currentDepth = 1; currentDepth < searchInfo.depth; currentDepth++) {
                int bestScore = RecursiveAlphaBeta(-Infinite, Infinite, currentDepth, board, searchInfo, true);
                // if (OutOfTime()) // previousBestMove
                int pvMoves = board.pvTable.GetPvLineCount(currentDepth);
                Move bestMove = board.pvArray[0];
                Debug.Log("Depth: " + currentDepth + " - move: " + BoardSquares.GetAlgebraicMove(bestMove.move) + " - nodes: " + searchInfo.nodes);
                /*for (int pvNum = 0; pvNum < pvMoves; pvNum++) {
                    Move move = board.pvArray[pvNum];
                    Debug.Log("PV move " + pvNum + ": " + Move.GetMoveString(move));
                }*/
            }
        }
    }
}
