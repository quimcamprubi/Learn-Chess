using System.Collections.Generic;
using UnityEngine.Assertions;
using Utils;

namespace Core {
    public class PVTable {
        public Entry[] entries;
        public readonly ulong size;
        private Board board;

        public struct Entry {
            public readonly ulong positionKey;
            public readonly Move move;

            public Entry(ulong positionKey, Move move) {
                this.positionKey = positionKey;
                this.move = move;
            }
        }

        public PVTable (Board board, int size) {
            this.size = (ulong) size;
            entries = new Entry[size];
            this.board = board;
        }
        
        public void Clear () {
            for (int i = 0; i < entries.Length; i++) {
                entries[i] = new Entry ();
            }
        }

        public void StorePvMove(Move move) {
            ulong index = board.positionKey % size;
            Assert.IsTrue(index <= size - 1, "StorePvMove index is too large.");
            entries[index] = new Entry(board.positionKey, move);
        }


        public Move ProbePvTable() {
            ulong index = board.positionKey % size;
            Assert.IsTrue(index <= size - 1, "ProbePVTable index is too large.");
            return entries[index].move;
        }

        public int GetPvLineCount(int depth) {
            Assert.IsTrue(depth < Constants.MAX_DEPTH, "PV Depth is too large.");
            Move move = ProbePvTable();
            int count = 0;
            while (move != null && count < depth) {
                if (MoveGenerator.MoveExists(board, move)) {
                    board.MakeMove(move);
                    board.pvArray[count++] = move;
                } else {
                    break;
                }
                move = ProbePvTable();
            }
            while (board.ply > 0) {
                board.UnmakeMove();
            }
            return count;
        }
    }
}
