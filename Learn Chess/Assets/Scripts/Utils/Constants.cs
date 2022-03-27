using System.Collections.Generic;
using Core;

namespace Utils {
    public static class Constants {
        public static readonly int NUM_RANKS = 8;
        public static readonly int NUM_FILES = 8;
        public static readonly int NUM_SQUARES = 64;
        public static readonly int NUM_SQUARES_EXT = 120;
        public static readonly int TOTAL_DIFF_PIECES = 13;
        public static readonly int NUM_PIECE_VARIANTS = 3;
        public static readonly int NUM_KINGS = 2;
        public static readonly int PIECE_TYPE_VARIANTS = 3;
        public static readonly int MAX_GAME_MOVES = 2048;
        public static readonly int MAX_PIECES_OF_SAME_TYPE = 10;
        public static readonly int NUM_PLAYERS = 2;
        
        public static readonly string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public static readonly string fen1 = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
        public static readonly string fen2 = "rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2";
        public static readonly string fen3 = "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2";
        public static readonly string fen4 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        public static readonly string sqAttackTestFen = "8/3q1p2/8/5P2/4Q3/8/8/8 w - - 0 2";
        public static readonly string whitePawnMovesFen = "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1";
        public static readonly string blackPawnMovesFen = "rnbqkbnr/p1p1p3/3p3p/1p1p4/2P1Pp2/8/PP1P1PpP/RNBQKB1R b KQkq e3 0 1";
        public static readonly string knightsKingsFen = "5k2/1n6/4n3/6N1/8/3N4/8/5K2 w - - 0 1";
        public static readonly string rookMovesFen = "6k1/8/5r2/8/1nR5/5N2/8/6K1 w - - 0 1";
        public static readonly string queenMovesFen = "6k1/8/4nq2/8/1nQ5/5N2/1N6/6K1 w - - 0 1";
        public static readonly string bishopMovesFen = "6k1/1b6/4n3/8/1n4B1/1B3N2/1N6/2b3K1 w - - 0 1";
        public static readonly string castlingFen1 = "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1";
        public static readonly string castlingFen2 = "3rk2r/8/8/8/8/8/6p1/R3K2R w KQk - 0 1";
        public static readonly string globalMoveGenFen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        
        public enum RanksEnum { RANK_1 = 0, RANK_2, RANK_3, RANK_4, RANK_5, RANK_6, RANK_7, RANK_8 }
        public enum FilesEnum { FILE_A = 0, FILE_B, FILE_C, FILE_D, FILE_E, FILE_F, FILE_G, FILE_H }
    }
}
