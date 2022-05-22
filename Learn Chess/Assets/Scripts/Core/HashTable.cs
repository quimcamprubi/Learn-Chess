using System.Collections;
using UnityEngine;
using Utils;

namespace Core {
    public class HashTable {
        public HashEntry[] hashEntries;
        public int numEntries;
        public int newWrite;
        public int overWrite;
        public int hit;
        public int cut;
        public readonly ulong size;
        private Board board;

        public enum FlagsEnum { HFNONE = 0, HFALPHA = 1, HFBETA = 2, HFEXACT = 3 }
        public struct HashEntry {
            public ulong positionKey;
            public Move move;
            public int score;
            public int depth;
            public int flags;

            public HashEntry(ulong positionKey, Move move, int score, int depth, int flags) {
                this.positionKey = positionKey;
                this.move = move;
                this.score = score;
                this.depth = depth;
                this.flags = flags;
            }
        }

        public HashTable (Board board, int size) {
            this.size = (ulong) size;
            hashEntries = new HashEntry[size];
            this.board = board;
        }
        
        public void Clear () {
            overWrite = 0;
            hit = 0;
            cut = 0;
        }

        public void StoreHashEntry(Move move, int score, int flags, int depth) {
            ulong index = board.positionKey % size;
            Debug.Assert(index <= size - 1, "HashTable index is too large.");
            Debug.Assert(depth >= 1 && depth <= Constants.MAX_DEPTH);
            Debug.Assert(board.ply >= 0 && board.ply < Constants.MAX_DEPTH);
            Debug.Assert(flags >= (int) HashTable.FlagsEnum.HFNONE && flags <= (int) HashTable.FlagsEnum.HFEXACT);
            if (hashEntries[index].positionKey == 0) {
                newWrite++;
            } else {
                overWrite++;
            }
            if (score > IsMate()) score += board.ply;
            else if (score < -IsMate()) score -= board.ply;
            /*if (score == Search.Infinite || score == -Search.Infinite) {
                Debug.Log("trouble");
            }*/
            hashEntries[index] = new HashEntry(board.positionKey, move, score, depth, flags);
        }

        public bool ProbeHashTable(Move pvMove, ref int score, int alpha, int beta, int depth) {
            ulong index = board.positionKey % size;
            Debug.Assert(index <= size - 1, "HashTable index is too large.");
            Debug.Assert(depth >= 1 && depth <= Constants.MAX_DEPTH);
            Debug.Assert(board.ply >= 0 && board.ply < Constants.MAX_DEPTH);
            Debug.Assert(alpha < beta);
            HashEntry entry = hashEntries[index];
            if (entry.positionKey == board.positionKey) {
                pvMove.move = entry.move.move;
                if (entry.depth >= depth) {
                    hit++;
                    Debug.Assert(entry.depth >= 1 && entry.depth <= Constants.MAX_DEPTH);
                    Debug.Assert(entry.flags >= (int) HashTable.FlagsEnum.HFNONE && entry.flags <= (int) HashTable.FlagsEnum.HFEXACT);
                    score = entry.score;
                    if (score > IsMate()) score -= board.ply;
                    else if (score < -IsMate()) score += board.ply;
                    switch (entry.flags) {
                        case (int) FlagsEnum.HFALPHA:
                            if (score <= alpha) {
                                score = alpha;
                                return true;
                            }
                            break;
                            
                        case (int) FlagsEnum.HFBETA:
                            if (score >= beta) {
                                score = beta;
                                return true;
                            }
                            break;
                        case (int) FlagsEnum.HFEXACT:
                            return true;
                    }
                }
            }
            return false;
        }
        
        public Move ProbeHashMove() {
            ulong index = board.positionKey % size;
            if (hashEntries[index].positionKey == board.positionKey)
                return hashEntries[index].move;
            return null;
        }
        
        public HashEntry GetEntry() {
            ulong index = board.positionKey % size;
            if (hashEntries[index].positionKey == board.positionKey)
                return hashEntries[index];
            return new HashEntry();
        }
        
        public int GetPvLineCount(int depth) {
            Debug.Assert(depth < Constants.MAX_DEPTH, "PV Depth is too large.");
            Move move = ProbeHashMove();
            int count = 0;
            while (move != null && count < depth) {
                if (MoveGenerator.MoveExists(board, move)) {
                    board.MakeMove(move);
                    board.pvArray[count++] = move;
                } else {
                    break;
                }
                move = ProbeHashMove();
            }
            while (board.ply > 0) {
                board.UnmakeMove();
            }
            return count;
        }

        public static int IsMate() { return Search.Infinite - Constants.MAX_DEPTH; }
    }
}
