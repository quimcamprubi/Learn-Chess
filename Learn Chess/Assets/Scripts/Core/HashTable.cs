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

        public int StoreHashEntry(Move move, int score, int flags, int depth) {
            ulong index = board.positionKey % size;
            Debug.Assert(index <= size - 1, "HashTable index is too large.");
            Debug.Assert(depth >= 1 && depth <= Constants.MAX_DEPTH);
            Debug.Assert(board.ply >= 0 && board.ply < Constants.MAX_DEPTH);
            Debug.Assert(flags >= (int) HashTable.FlagsEnum.HFNONE && flags <= (int) HashTable.FlagsEnum.HFEXACT);
            int returnScore = score;
            if (hashEntries[index].positionKey == 0) {
                newWrite++;
            } else {
                overWrite++;
            }
            if (score > IsMate()) returnScore += board.ply;
            else if (score < -IsMate()) returnScore -= board.ply;
            hashEntries[index] = new HashEntry(board.positionKey, move, score, depth, flags);
            return returnScore;
        }

        public bool ProbeHashTable(Move move, ref int score, int alpha, int beta, int depth) {
            ulong index = board.positionKey % size;
            Debug.Assert(index <= size - 1, "HashTable index is too large.");
            Debug.Assert(depth >= 1 && depth <= Constants.MAX_DEPTH);
            Debug.Assert(board.ply >= 0 && board.ply < Constants.MAX_DEPTH);
            Debug.Assert(alpha < beta);
            HashEntry entry = hashEntries[index];
            if (entry.positionKey == board.positionKey) {
                move.move = entry.move.move;
                if (entry.depth >= depth) {
                    hit++;
                    Debug.Assert(entry.depth >= 1 && entry.depth <= Constants.MAX_DEPTH);
                    Debug.Assert(entry.flags >= (int) HashTable.FlagsEnum.HFNONE && entry.flags <= (int) HashTable.FlagsEnum.HFEXACT);
                    Debug.Assert(entry.score >= -Search.Infinite && entry.score <= Search.Infinite);
                    move.score = entry.score;
                    if (move.score > IsMate()) move.score -= board.ply;
                    else if (move.score < -IsMate()) move.score += board.ply;
                    switch (entry.flags) {
                        case (int) FlagsEnum.HFALPHA:
                            score = alpha;
                            return true;
                        case (int) FlagsEnum.HFBETA:
                            score = beta;
                            return true;
                        case (int) FlagsEnum.HFEXACT:
                            return true;
                    }
                }
            }
            return false;
        }
        
        public Move ProbeHashEntry() {
            ulong index = board.positionKey % size;
            Debug.Assert(index <= size - 1, "ProbePVTable index is too large.");
            return hashEntries[index].move;
        }
        
        public int GetPvLineCount(int depth) {
            Debug.Assert(depth < Constants.MAX_DEPTH, "PV Depth is too large.");
            Move move = ProbeHashEntry();
            int count = 0;
            while (move != null && count < depth) {
                if (MoveGenerator.MoveExists(board, move)) {
                    board.MakeMove(move);
                    board.pvArray[count++] = move;
                } else {
                    break;
                }
                move = ProbeHashEntry();
            }
            while (board.ply > 0) {
                board.UnmakeMove();
            }
            return count;
        }

        public static int IsMate() { return Search.Infinite - Constants.MAX_DEPTH; }
    }
}
