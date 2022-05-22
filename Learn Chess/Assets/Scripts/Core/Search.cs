
using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using Utils;
using Debug = UnityEngine.Debug;

namespace Core {
    public class SearchInfo {
        public DateTime startTime;
        public DateTime stopTime;
        public int durationSet;
        public int depth;
        public bool depthSet;
        public bool timeSet;
        public int movesToGo;
        public long nodes;
        public bool quit;
        public bool stopped;
        public bool infinite;
        public bool transposition;
        public bool quiescence;
        public float failHigh;
        public float failHighFirst;

        public SearchInfo(int depth) {
            this.depth = depth;
            timeSet = true;
            stopped = false;
            durationSet = 10;
            transposition = true;
            quiescence = true;
        }

        public SearchInfo(int depth, bool timeSet = true, int durationSet = 10, bool transposition = true, bool quiescence = true) {
            this.depth = depth;
            this.timeSet = timeSet;
            this.durationSet = durationSet;
            stopped = false;
            this.transposition = transposition;
            this.quiescence = quiescence;
        }
        
        public void ClearSearchData() {
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
        
        public static void CheckFinish(SearchInfo searchInfo) {
            DateTime now = DateTime.Now;
            if (searchInfo.timeSet && (now - searchInfo.startTime).TotalSeconds > searchInfo.durationSet) {
                searchInfo.stopped = true;
                //searchInfo.stopTime = now;
            }
        }
        
        public static bool IsRepeated(Board board) {
            for (int i = board.histPly - board.fiftyMoveCounter; i < board.histPly - 1; i++) {
                if (board.gameHist[i].positionKey == board.positionKey) return true;
            }
            return false;
        }

        public static void ClearForSearch(Board board, SearchInfo searchInfo) {
            board.ClearSearchData(searchInfo.transposition);
            searchInfo.ClearSearchData();
        } 
        
        public static int QuiescenceSearch(int alpha, int beta, Board board, SearchInfo searchInfo) { // Same as AlphaBeta, but with additional search after capture
            Debug.Assert(board.CheckBoard());
            if (searchInfo.nodes > 0 && (searchInfo.nodes & 2047) == 0) { // Every 2048 nodes, we check whether we've run out of time or not
                CheckFinish(searchInfo);
            }
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

                if (searchInfo.stopped) return 0;
                
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
            if (depth <= 0) {
                if (quiescenceSearch) return QuiescenceSearch(alpha, beta, board, searchInfo);
                return Evaluate.EvaluatePosition(board);
            }
            
            if (searchInfo.nodes > 0 && (searchInfo.nodes & 2047) == 0) { // Every 2048 nodes, we check whether we've run out of time or not
                CheckFinish(searchInfo);
            }
            
            searchInfo.nodes++;
            
            if (IsRepeated(board) || board.fiftyMoveCounter >= 100 && board.ply != 0) {
                return 0;
            }
            
            if (board.ply > Constants.MAX_DEPTH - 1) {
                return Evaluate.EvaluatePosition(board);
            }

            bool inCheck = board.IsKingInCheck();
            if (inCheck) {
                depth++;
            }
            
            int score = -Infinite;
            Move pvMove = new Move();
            if (searchInfo.transposition) {
                bool hasHit =  board.hashTable.ProbeHashTable(pvMove, ref score, alpha, beta, depth);
                if (hasHit) {
                    board.hashTable.cut++;
                    return score;
                }
            }
            
            if (nullMove && !inCheck && board.ply != 0 && board.bigPieces[board.sideToPlay] > 1 && depth >= 4) {
                board.MakeNullMove();
                score = -RecursiveAlphaBeta(-beta, -beta + 1, depth - 4, board, searchInfo, nullMove: false);
                board.UnmakeNullMove();
                if (searchInfo.stopped) {
                    return 0;
                }
                if (score >= beta) {
                    return beta;
                }
            }
            
            int legalMoves = 0;
            int oldAlpha = alpha;
            Move bestMove = new Move(0, -Infinite);
            Move[] movesList = MoveGenerator.GenerateAllMoves(board).ToArray();
            if (!searchInfo.transposition) pvMove = board.pvTable.ProbePvTable();
            int bestScore = -Infinite;
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
                score = -RecursiveAlphaBeta(-beta, -alpha, depth - 1, board, searchInfo, true); // Negative because it is a Negamax implementation
                board.UnmakeMove();
                if (searchInfo.stopped) return 0; // Iterative deepening time limit
                if (score > bestScore) {
                    bestScore = score;
                    bestMove = move;
                    if (score > alpha) {
                        if (score >= beta) {
                            if (legalMoves == 1) searchInfo.failHighFirst++;
                            searchInfo.failHigh++;
                            if (!Move.IsMoveCapture(move.move)) {
                                board.searchKillers[1, board.ply] = board.searchKillers[0, board.ply];
                                board.searchKillers[0, board.ply] = move.move;
                            }
                            if (searchInfo.transposition) board.hashTable.StoreHashEntry(bestMove, beta, (int) HashTable.FlagsEnum.HFBETA, depth);
                            return beta;
                        }
                        alpha = score;
                        //bestMove = move;
                        if (!Move.IsMoveCapture(move.move)) {
                            board.searchHistory[board.squares[Move.FromSquare(move.move)], board.squares[Move.ToSquare(move.move)]] += depth;
                        }
                    }
                }
            }
                
            
            if (legalMoves == 0) {
                if (inCheck) {
                    return -Infinite + board.ply; // Checkmate, taking into account how far it is
                }
                return 0; // Stalemate
            }
            
            if (alpha != oldAlpha) {
                if (searchInfo.transposition) {
                    board.hashTable.StoreHashEntry(bestMove, bestScore, (int) HashTable.FlagsEnum.HFEXACT, depth);
                }
                else board.pvTable.StorePvMove(bestMove);
            } else {
                if (searchInfo.transposition) board.hashTable.StoreHashEntry(bestMove, alpha, (int) HashTable.FlagsEnum.HFALPHA, depth);
            }
            return alpha;
        }
        
        public static void SearchPosition(Board board, SearchInfo searchInfo, Game game, bool nullMove = true, bool printAllData = false) { // Iterative deepening function
            Move bestMove = new Move(Board.None, -Infinite);
            ClearForSearch(board, searchInfo);
            StringBuilder sb = new StringBuilder();
            searchInfo.startTime = DateTime.Now;
            int bestScore = -Infinite;
            for (int currentDepth = 1; currentDepth <= searchInfo.depth; currentDepth++) {
                bestScore = RecursiveAlphaBeta(-Infinite, Infinite, currentDepth, board, searchInfo, nullMove, searchInfo.quiescence);
                if (searchInfo.stopped) break; // If out of time, break out of the loop
                if (searchInfo.transposition) board.hashTable.GetPvLineCount(currentDepth);
                else board.pvTable.GetPvLineCount(currentDepth);
                bestMove = board.pvArray[0];
                game.currentPlayerEvaluation = board.sideToPlay == Board.Black ? -bestScore/100f : bestScore/100f;
                if (bestMove != null) {
                    sb.Append("Depth: " + currentDepth + "  score: " + bestScore + "  best move: " + BoardSquares.GetAlgebraicMove(bestMove.move) + "  nodes: " + searchInfo.nodes + "\n");
                    sb.AppendLine();
                    game.suggestedMove = bestMove;
                }
            }
            if (printAllData) Debug.Log(sb.ToString());
            searchInfo.stopTime = DateTime.Now;
            if (printAllData) Debug.Log("Best move found: " + Move.GetMoveString(bestMove) + " after " + (searchInfo.stopTime - searchInfo.startTime).TotalSeconds + " seconds.");
        }
        
        public static void QuickSearch(Board board, SearchInfo searchInfo, Game game, bool nullMove = true, bool printAllData = false) { // Iterative deepening function
            Move bestMove = new Move(Board.None, -Infinite);
            ClearForSearch(board, searchInfo);
            searchInfo.startTime = DateTime.Now;
            int bestScore = RecursiveAlphaBeta(-Infinite, Infinite, searchInfo.depth, board, searchInfo, nullMove, searchInfo.quiescence);
            if (searchInfo.stopped) return; // If out of time, break out of the loop
            if (searchInfo.transposition) board.hashTable.GetPvLineCount(searchInfo.depth);
            else board.pvTable.GetPvLineCount(searchInfo.depth);
            bestMove = board.pvArray[0];
            game.currentPlayerEvaluation = board.sideToPlay == Board.Black ? -bestScore/100f : bestScore/100f;
            if (bestMove != null) {
                if (printAllData) Debug.Log("Depth: " + searchInfo.depth + "  score: " + bestScore + "  best move: " + BoardSquares.GetAlgebraicMove(bestMove.move) + "  nodes: " + searchInfo.nodes + "\n");
                game.suggestedMove = bestMove;
                searchInfo.stopTime = DateTime.Now;
                if (printAllData) Debug.Log("Best move found: " + BoardSquares.GetAlgebraicMove(bestMove.move) + " after " + (searchInfo.stopTime - searchInfo.startTime).TotalSeconds + " seconds.");
            }
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
