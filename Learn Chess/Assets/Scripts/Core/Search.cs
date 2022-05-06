﻿
using System.Diagnostics;
using System.Text;
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
        public float failHigh;
        public float failHighFirst;

        public SearchInfo(int depth) {
            this.depth = depth;
        }
        
        public void ClearSearchData() {
            startTime = (int) Stopwatch.GetTimestamp();
            stopped = false;
            nodes = 0;
            failHigh = 0;
            failHighFirst = 0;
        }
    }
    
    public static class Search {
        public static int Infinite = 30000;
        public static int Mate = 29000;
        public static int[,] MvvLvaScores = new int[13, 13];

        public static void InitMvvLva() {
            for (int atk = Piece.WhitePawn; atk <= Piece.BlackKing; atk++) {
                for (int vic = Piece.WhitePawn; vic <= Piece.BlackKing; vic++) {
                    MvvLvaScores[vic, atk] = Piece.VictimScores[vic] + 6 - (Piece.VictimScores[atk] / 100);
                }
            }
            /*for (int vic = Piece.WhitePawn; vic <= Piece.BlackKing; vic++) {
                for (int atk = Piece.WhitePawn; atk <= Piece.BlackKing; atk++) {
                    Debug.Log(Piece.PieceStrings[atk] + " x " + Piece.PieceStrings[vic] + " = " + MvvLvaScores[vic, atk]);
                }
            }*/
        }
        
        public static void CheckFinish() {
            // Check if Search has been running for too long
        }
        
        public static bool IsRepeated(Board board) {
            for (int i = board.histPly - board.fiftyMoveCounter; i < board.histPly - 1; i++) {
                if (board.gameHist[i].positionKey == board.positionKey) return true;
            }
            return false;
        }

        public static void ClearForSearch(Board board, SearchInfo searchInfo) {
            board.ClearSearchData();
            searchInfo.ClearSearchData();
        } 
        
        public static int QuiescenceSearch(int alpha, int beta, Board board, SearchInfo searchInfo) { // Same as AlphaBeta, but with additional search after capture
            Debug.Assert(board.CheckBoard());
            searchInfo.nodes++;
            if (IsRepeated(board) || board.fiftyMoveCounter >= 100) {
                return 0;
            }
            if (board.ply > Constants.MAX_DEPTH - 1) {
                return Evaluate.EvaluatePosition(board);
            }
            
            int score = Evaluate.EvaluatePosition(board);
            if (score >= beta) {
                return beta;
            }
            if (score > alpha) {
                alpha = score; 
            }
            
            int legalMoves = 0;
            int oldAlpha = alpha;
            Move bestMove = new Move(0, -Infinite);
            Move[] movesList = MoveGenerator.GenerateAllCaptures(board).ToArray();
            Move pvMove = board.pvTable.ProbePvTable();
            for (int moveNumber = 0; moveNumber < movesList.Length; moveNumber++) {
                NextMove(moveNumber, movesList); // We choose the best move first, implementing move ordering, improving Alpha Beta cutoffs.
                Move move = movesList[moveNumber];
                if (!board.MakeMove(move)) continue;
                legalMoves++;
                score = -QuiescenceSearch(-beta, -alpha, board, searchInfo); // Negative because it is a Negamax implementation
                board.UnmakeMove();
                if (score > alpha) {
                    if (score >= beta) {
                        if (legalMoves == 1) searchInfo.failHighFirst++;
                        searchInfo.failHigh++;
                        return beta;
                    }
                    alpha = score;
                    bestMove = move;
                }
            }
            if (alpha != oldAlpha) {
                board.pvTable.StorePvMove(bestMove);
            }
            return alpha;
        }
        
        public static int RecursiveAlphaBeta(int alpha, int beta, int depth, Board board, SearchInfo searchInfo, bool nullMove = false, bool quiescenceSearch = true) {
            //Debug.Assert(board.CheckBoard(), "Checkboard failed at RecursiveAlphaBeta");
            if (depth == 0) {
                if (quiescenceSearch) return QuiescenceSearch(alpha, beta, board, searchInfo);
                return Evaluate.EvaluatePosition(board);
            }
            searchInfo.nodes++;
            
            if (IsRepeated(board) || board.fiftyMoveCounter >= 100 && board.ply != 0) {
                return 0;
            }
            
            if (board.ply > Constants.MAX_DEPTH - 1) {
                return Evaluate.EvaluatePosition(board);
            }
            
            int legalMoves = 0;
            int oldAlpha = alpha;
            Move bestMove = new Move(0, -Infinite);
            Move[] movesList = MoveGenerator.GenerateAllMoves(board).ToArray();
            Move pvMove = board.pvTable.ProbePvTable();
            if (pvMove != null) {
                // Main line, PV move
                foreach (Move move in movesList) {
                    if (move.move == pvMove.move) {
                        move.score = 2000000;
                        break;
                    }
                }
            }
            for (int moveNumber = 0; moveNumber < movesList.Length; moveNumber++) {
                NextMove(moveNumber, movesList); // We choose the best move first, implementing move ordering, improving Alpha Beta cutoffs.
                Move move = movesList[moveNumber];
                if (!board.MakeMove(move)) continue;
                legalMoves++;
                int score = -RecursiveAlphaBeta(-beta, -alpha, depth - 1, board, searchInfo, true); // Negative because it is a Negamax implementation
                board.UnmakeMove();
                if (score > alpha) {
                    if (score >= beta) {
                        if (legalMoves == 1) searchInfo.failHighFirst++;
                        searchInfo.failHigh++;
                        if (!Move.IsMoveCapture(move.move)) {
                            board.searchKillers[1, board.ply] = board.searchKillers[0, board.ply];
                            board.searchKillers[0, board.ply] = move.move;
                        }
                        return beta;
                    }
                    alpha = score;
                    bestMove = move;
                    if (!Move.IsMoveCapture(move.move)) {
                        board.searchHistory[board.squares[Move.FromSquare(move.move)], board.squares[Move.ToSquare(move.move)]] += depth;
                    }
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
            StringBuilder sb = new StringBuilder();
            for (int currentDepth = 1; currentDepth < searchInfo.depth + 1; currentDepth++) {
                int bestScore = RecursiveAlphaBeta(-Infinite, Infinite, currentDepth, board, searchInfo, true);
                // if (OutOfTime()) // previousBestMove
                board.pvTable.GetPvLineCount(currentDepth);
                Move bestMove = board.pvArray[0];
                sb.Append("Depth: " + currentDepth + "  score: " + bestScore + "  best move: " + BoardSquares.GetAlgebraicMove(bestMove.move) + "  nodes: " + searchInfo.nodes + "\n");
                float ordering = searchInfo.failHighFirst == 0 && searchInfo.failHigh == 0 ? 0 : searchInfo.failHighFirst / searchInfo.failHigh;
                sb.Append("Ordering: " + ordering + "\n");
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }

        public static void NextMove(int moveNumber, Move[] movesList) {
            int bestScore = 0;
            int bestMove = moveNumber;
            for (int i = moveNumber; i < movesList.Length; i++) {
                if (movesList[i].score > bestScore) {
                    bestScore = movesList[i].score;
                    bestMove = i;
                }
            }
            Move temp = movesList[moveNumber];
            movesList[moveNumber] = movesList[bestMove];
            movesList[bestMove] = temp;
        }
    }
}
