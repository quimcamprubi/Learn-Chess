namespace Utils {
    public static class BitBoardUtils {
        private static readonly int[] BitTable = {
            63, 30, 3, 32, 25, 41, 22, 33, 15, 50, 42, 13, 11, 53, 19, 34, 61, 29, 2,
            51, 21, 43, 45, 10, 18, 47, 1, 54, 9, 57, 0, 35, 62, 31, 40, 4, 49, 5, 52,
            26, 60, 6, 23, 44, 46, 27, 56, 16, 7, 39, 48, 24, 59, 14, 12, 55, 38, 28,
            58, 20, 37, 17, 36, 8
        };
        
        public static int PopBit(ref ulong bitBoard) {
            ulong b = bitBoard ^ (bitBoard - 1);
            uint fold = (uint) ((b & 0xffffffff) ^ (b >> 32));
            bitBoard &= (bitBoard - 1);
            return BitTable[(fold * 0x783a9b23) >> 26];
        }

        public static int CountBitBoard(ulong bitBoard) {
            int count = 0;
            while (bitBoard != 0) {
                count += (int) bitBoard & 1;
                bitBoard >>= 1;
            }
            return count;
        }
        
        public static bool ContainsSquare (ulong bitboard, int square) {
            return ((bitboard >> square) & 1) != 0;
        }
    }
}
