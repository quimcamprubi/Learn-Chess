using System;

namespace Core {
    public class Coordinates : IComparable<Coordinates> {
        public readonly int fileIndex;
        public readonly int rankIndex;

        public Coordinates(int fileIndex, int rankIndex) {
            this.fileIndex = fileIndex;
            this.rankIndex = rankIndex;
        }

        public bool IsLightSquare() { return (fileIndex + rankIndex) % 2 != 0; }
        public int CompareTo(Coordinates other) { return (fileIndex == other.fileIndex && rankIndex == other.rankIndex) ? 0 : 1; }
    }
}
