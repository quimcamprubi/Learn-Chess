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

        public void StorePvMove(Board board, Move move) {
            ulong index = board.positionKey % size;
            Assert.IsTrue(index <= size - 1);
            entries[index] = new Entry(board.positionKey, move);
        }


        public Move ProbePvTable(Board board) {
            ulong index = board.positionKey % size;
            Assert.IsTrue(index <= size - 1);
            if (entries[index].positionKey == board.positionKey) {
                return entries[index].move;
            }
            return null;
        }
    }
}
