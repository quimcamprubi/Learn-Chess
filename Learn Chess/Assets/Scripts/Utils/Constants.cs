using Core;

namespace Utils
{
    public static class Constants
    {
        public static int NUM_RANKS = 8;
        public static int NUM_FILES = 8;
        public static int NUM_SQUARES = 64;
        public static int NUM_SQUARES_EXT = 120;
        public static int TOTAL_DIFF_PIECES = 13;
        public static int NUM_PIECE_VARIANTS = 3;
        public static int NUM_KINGS = 2;
        public static int PIECE_TYPE_VARIANTS = 3;
        public static int MAX_GAME_MOVES = 2048;
        public static int MAX_PIECES_OF_SAME_TYPE = 10;
        public static int NUM_PLAYERS = 2;
        
        public static string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq d3 0 1";
    }
}
