using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Core {
    public class PVTable {
        public Entry[] entries;
        public readonly ulong size;

        public struct Entry {
            public readonly ulong positionKey;
            public readonly Move move;

            public Entry(ulong positionKey, Move move) {
                this.positionKey = positionKey;
                this.move = move;
            }
        }

        public PVTable (int size) {
            this.size = (ulong) size;
            entries = new Entry[size];
        }
        
        public void Clear () {
            for (int i = 0; i < entries.Length; i++) {
                entries[i] = new Entry ();
            }
        }

        public void StorePvMove(ulong positionKey, Move move) {
            ulong index = positionKey % size;
            Assert.IsTrue(index <= size - 1, "StorePvMove index is too large.");
            entries[index] = new Entry(positionKey, move);
        }


        public Move ProbePvTable(ulong positionKey) {
            ulong index = positionKey % size;
            Assert.IsTrue(index <= size - 1, "ProbePVTable index is too large.");
            return entries[index].move;
        }

        public int GetPvLineCount(int depth, Board board) {
            Assert.IsTrue(depth < Search.MaxDepth, "PV Depth is too large.");
            Move move = ProbePvTable(board.positionKey);
            int count = 0;
            while (move != null && count < depth) {
                if (MoveGenerator.MoveExists(board, move)) {
                    board.MakeMove(move);
                    board.pvArray[count++] = move;
                } else {
                    break;
                }
                move = ProbePvTable(board.positionKey);
            }
            while (board.ply > 0) {
                board.UnmakeMove();
            }
            return count;
        }
    }
}
