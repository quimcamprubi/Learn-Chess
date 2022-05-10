using UnityEngine;
using UnityEngine.Assertions;

namespace Core {
    public static class Evaluate {
        // Evaluation tables. These tables indicate where the engine should intend to put its pieces by giving them more
        // value to said pieces if placed on the squares with higher scores. These tables generally try to move pieces to the
        // center of the board, developing the advanced pieces and valuing central pawns higher than otherwise.

        public static readonly int IsolatedPawn = -10;  // Isolated pawns (undefended by other pawns) are less valuable.
        public static readonly int[] AdvancedPawn = new int[8] {0, 5, 10, 20, 35, 60, 100, 200}; // the more the pawn advances, the higher its value.
        public static readonly int RookOpenFile = 10;    // Rooks controlling open files are valuable.
        public static readonly int RookSemiOpenFile = 5;
        public static readonly int QueenOpenFile = 5;
        public static readonly int QueenSemiOpenFile = 3;
        
        public static readonly int[] PawnTable = {
            0	,	0	,	0	,	0	,	0	,	0	,	0	,	0	,
            10	,	10	,	0	,	-10	,	-10	,	0	,	10	,	10	,
            5	,	0	,	0	,	5	,	5	,	0	,	0	,	5	,
            0	,	0	,	10	,	20	,	20	,	10	,	0	,	0	,
            5	,	5	,	5	,	10	,	10	,	5	,	5	,	5	,
            10	,	10	,	10	,	20	,	20	,	10	,	10	,	10	,
            20	,	20	,	20	,	30	,	30	,	20	,	20	,	20	,
            0	,	0	,	0	,	0	,	0	,	0	,	0	,	0	
        };

        public static readonly int[] KnightTable = {
            0	,	-10	,	0	,	0	,	0	,	0	,	-10	,	0	,
            0	,	0	,	0	,	5	,	5	,	0	,	0	,	0	,
            0	,	0	,	10	,	10	,	10	,	10	,	0	,	0	,
            0	,	0	,	10	,	20	,	20	,	10	,	0	,	0	,
            5	,	10	,	15	,	20	,	20	,	15	,	10	,	5	,
            5	,	10	,	10	,	20	,	20	,	10	,	10	,	5	,
            0	,	0	,	5	,	10	,	10	,	5	,	0	,	0	,
            0	,	0	,	0	,	0	,	0	,	0	,	0	,	0		
        };

        public static readonly int[] BishopTable = {
            0	,	0	,	-10	,	0	,	0	,	-10	,	0	,	0	,
            0	,	0	,	0	,	10	,	10	,	0	,	0	,	0	,
            0	,	0	,	10	,	15	,	15	,	10	,	0	,	0	,
            0	,	10	,	15	,	20	,	20	,	15	,	10	,	0	,
            0	,	10	,	15	,	20	,	20	,	15	,	10	,	0	,
            0	,	0	,	10	,	15	,	15	,	10	,	0	,	0	,
            0	,	0	,	0	,	10	,	10	,	0	,	0	,	0	,
            0	,	0	,	0	,	0	,	0	,	0	,	0	,	0	
        };

        public static readonly int[] RookTable = {
            0	,	0	,	5	,	10	,	10	,	5	,	0	,	0	,
            0	,	0	,	5	,	10	,	10	,	5	,	0	,	0	,
            0	,	0	,	5	,	10	,	10	,	5	,	0	,	0	,
            0	,	0	,	5	,	10	,	10	,	5	,	0	,	0	,
            0	,	0	,	5	,	10	,	10	,	5	,	0	,	0	,
            0	,	0	,	5	,	10	,	10	,	5	,	0	,	0	,
            25	,	25	,	25	,	25	,	25	,	25	,	25	,	25	,
            0	,	0	,	5	,	10	,	10	,	5	,	0	,	0		
        };

        public static readonly int[] Mirror64 = {
            56	,	57	,	58	,	59	,	60	,	61	,	62	,	63	,
            48	,	49	,	50	,	51	,	52	,	53	,	54	,	55	,
            40	,	41	,	42	,	43	,	44	,	45	,	46	,	47	,
            32	,	33	,	34	,	35	,	36	,	37	,	38	,	39	,
            24	,	25	,	26	,	27	,	28	,	29	,	30	,	31	,
            16	,	17	,	18	,	19	,	20	,	21	,	22	,	23	,
            8	,	9	,	10	,	11	,	12	,	13	,	14	,	15	,
            0	,	1	,	2	,	3	,	4	,	5	,	6	,	7
        };
        
        public static int EvaluatePosition(Board board) { // Evaluation from White's POV
            int score = board.material[Board.White] - board.material[Board.Black];
            // Pawns
            int piece = Piece.WhitePawn;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                score += PawnTable[Board.Sq64(square)];
                if ((board.isolatedMask[Board.Sq64(square)] & board.pawns[Board.White]) == 0) { // If the pawn is isolated, it's worth less
                    score += IsolatedPawn;
                }
                if ((board.whitePassedMask[Board.Sq64(square)] & board.pawns[Board.Black]) == 0) { // If the pawn has passed enemy pawns, it's more valuable
                    score += AdvancedPawn[Board.squareRank[square]];
                }
            }
            piece = Piece.BlackPawn;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                score -= PawnTable[Mirror64[Board.Sq64(square)]];
                if ((board.isolatedMask[Board.Sq64(square)] & board.pawns[Board.Black]) == 0) { // If the pawn is isolated, it's worth less
                    score -= IsolatedPawn;
                }
                if ((board.blackPassedMask[Board.Sq64(square)] & board.pawns[Board.White]) == 0) { // If the pawn has passed enemy pawns, it's more valuable
                    score -= AdvancedPawn[7 - Board.squareRank[square]];
                }
            }
            
            // Knights
            piece = Piece.WhiteKnight;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                score += KnightTable[Board.Sq64(square)];
            }
            piece = Piece.BlackKnight;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                score -= KnightTable[Mirror64[Board.Sq64(square)]];
            }
            
            // Bishops
            piece = Piece.WhiteBishop;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                score += BishopTable[Board.Sq64(square)];
            }
            piece = Piece.BlackBishop;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                score -= BishopTable[Mirror64[Board.Sq64(square)]];
            }
            
            // Rooks
            piece = Piece.WhiteRook;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                score += RookTable[Board.Sq64(square)];
                if ((board.pawns[Board.Both] & board.openFileMask[Board.squareFile[square]]) == 0) {
                    score += RookOpenFile;
                } else if ((board.pawns[Board.White] & board.openFileMask[Board.squareFile[square]]) == 0) {
                    score += RookSemiOpenFile;
                }
            }
            piece = Piece.BlackRook;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                score -= RookTable[Mirror64[Board.Sq64(square)]];
                if ((board.pawns[Board.Both] & board.openFileMask[Board.squareFile[square]]) == 0) {
                    score -= RookOpenFile;
                } else if ((board.pawns[Board.Black] & board.openFileMask[Board.squareFile[square]]) == 0) {
                    score -= RookSemiOpenFile;
                }
            }
            
            // Queens
            piece = Piece.WhiteQueen;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                if ((board.pawns[Board.Both] & board.openFileMask[Board.squareFile[square]]) == 0) {
                    score += QueenOpenFile;
                } else if ((board.pawns[Board.White] & board.openFileMask[Board.squareFile[square]]) == 0) {
                    score += QueenSemiOpenFile;
                }
            }
            piece = Piece.BlackQueen;
            for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                int square = board.pieceList[piece, pieceNumber];
                Assert.IsTrue(Validations.IsSquareOnBoard(square));
                if ((board.pawns[Board.Both] & board.openFileMask[Board.squareFile[square]]) == 0) {
                    score -= QueenOpenFile;
                } else if ((board.pawns[Board.Black] & board.openFileMask[Board.squareFile[square]]) == 0) {
                    score -= QueenSemiOpenFile;
                }
            }

            return board.sideToPlay == Board.White ? score : -score;
        }
    }
}
