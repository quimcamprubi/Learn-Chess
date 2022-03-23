using System;
using Utils;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;
using UI;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine.Assertions;

namespace Core
{
    public class Board
    {
        // Game board
        public int[] squares = new int[Constants.NUM_SQUARES_EXT];
        
        // Indices
        public static int[] sq64ToSq120 = new int[Constants.NUM_SQUARES];       // 64 indices to 120 indices
        public static int[] sq120ToSq64 = new int[Constants.NUM_SQUARES_EXT];   // 120 indices to 64 indices
        private int[] kingSquares = new int[Constants.NUM_KINGS];
        public static int[] squareFile = new int[Constants.NUM_SQUARES_EXT];
        public static int[] squareRank = new int[Constants.NUM_SQUARES_EXT];

        // Game rules
        public int sideToPlay;                                         
        public int enPassantSquare;                                            // EnPassant available square index
        public int fiftyMoveCounter;                                           // If it reaches 100 (half-moves), game is drawn
        public int ply;                                                        // Total half-moves played in game
        public int histPly;                                                    // gameHist index
        public int castlingRights;                                             // 4 bits for castling rights: WKCA WQCA BKCA BQCA
        private Move[] gameHist = new Move[Constants.MAX_GAME_MOVES];

        // Hash key
        public ulong positionKey;
        private ulong[,] pieceKeys = new ulong[Constants.TOTAL_DIFF_PIECES, Constants.NUM_SQUARES_EXT];
        private ulong sideKey;
        private ulong[] castleKeys = new ulong[16];
        
        // Piece lists
        private int[] pieceNumbers = new int[Constants.TOTAL_DIFF_PIECES];
        private int[] bigPieces = new int[Constants.NUM_PLAYERS];
        private int[] majorPieces = new int[Constants.NUM_PLAYERS];
        private int[] minorPieces = new int[Constants.NUM_PLAYERS];
        private int[] material = new int[Constants.NUM_PLAYERS];
        private int[,] pieceList = new int[Constants.TOTAL_DIFF_PIECES, Constants.MAX_PIECES_OF_SAME_TYPE];

        // Bitboard
        private ulong[] pawns = new ulong[Constants.NUM_PIECE_VARIANTS];
        private ulong[] setMask = new ulong[Constants.NUM_SQUARES];
        private ulong[] clearMask = new ulong[Constants.NUM_SQUARES];
        
        public static int None = 0; // 0 constant
        
        public static int White = 0;
        public static int Black = 1;
        public static int Both = 2;

        public enum Squares120Enum
        {
            A1 = 21, B1, C1, D1, E1, F1, G1, H1,
            A2 = 31, B2, C2, D2, E2, F2, G2, H2,
            A3 = 41, B3, C3, D3, E3, F3, G3, H3,
            A4 = 51, B4, C4, D4, E4, F4, G4, H4,
            A5 = 61, B5, C5, D5, E5, F5, G5, H5,
            A6 = 71, B6, C6, D6, E6, F6, G6, H6,
            A7 = 81, B7, C7, D7, E7, F7, G7, H7,
            A8 = 91, B8, C8, D8, E8, F8, G8, H8, NO_SQ, OFFBOARD
        }

        public enum SquaresEnum
        {
            A1 = 0, B1, C1, D1, E1, F1, G1, H1,
            A2 = 8, B2, C2, D2, E2, F2, G2, H2,
            A3 = 16, B3, C3, D3, E3, F3, G3, H3,
            A4 = 24, B4, C4, D4, E4, F4, G4, H4,
            A5 = 32, B5, C5, D5, E5, F5, G5, H5,
            A6 = 40, B6, C6, D6, E6, F6, G6, H6,
            A7 = 48, B7, C7, D7, E7, F7, G7, H7,
            A8 = 56, B8, C8, D8, E8, F8, G8, H8, NO_SQ
        }

        public void loadStartingPosition()
        {
            initBoards();
            resetBoard();
            initFromPosition(FenDecoder.DecodePositionFromFen(Constants.startingFen));
            printGameBoard();
            print120Board();
            printBitBoard(pawns[White], White);
            printBitBoard(pawns[Black], Black);
            printBitBoard(pawns[Both], Both);
        }

        public void loadPosition(ChessPosition position)
        {
            initBoards();
            resetBoard();
            initFromPosition(position);
            printGameBoard();
            print120Board();
            printBitBoard(pawns[White], White);
            printBitBoard(pawns[Black], Black);
            printBitBoard(pawns[Both], Both);
        }
        
        private void initBoards()
        {
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++)
            {
                sq120ToSq64[i] = 65;
                squareFile[i] = (int) Squares120Enum.OFFBOARD;
                squareRank[i] = (int) Squares120Enum.OFFBOARD;
            }

            for (int i = 0; i < Constants.NUM_SQUARES; i++)
            {
                sq64ToSq120[i] = 120;
                setMask[i] = 0UL;
                clearMask[i] = 0UL;
            }

            for (int i = 0; i < Constants.NUM_SQUARES; i++)
            {
                setMask[i] |= (1UL << i);
                clearMask[i] = ~setMask[i];
            }
            
            int square64 = 0;
            for (int rank = 0; rank < Constants.NUM_RANKS; rank++)
            {
                for (int file = 0; file < Constants.NUM_FILES; file++)
                {
                    int square = frTo120Sq(file, rank);
                    sq64ToSq120[square64] = square;
                    sq120ToSq64[square] = square64;
                    squareFile[square] = file;
                    squareRank[square] = rank;
                    square64++;
                }
            }
        }

        private void initFromPosition(ChessPosition loadedPosition)
        {
            squares = loadedPosition.squares;
            sideToPlay = loadedPosition.whiteToMove ? White : Black;
            castlingRights = loadedPosition.castlingRights;
            enPassantSquare = loadedPosition.enPassantSquare;
            ply = loadedPosition.plyCount;
            initHashKeys();
            positionKey = generatePositionKey();
            updateListsMaterial();
            checkBoard();
        }

        private void resetBoard()
        {
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++)
            {
                squares[i] = (int) Squares120Enum.OFFBOARD;
            }

            for (int i = 0; i < Constants.NUM_SQUARES; i++)
            {
                squares[sq120(i)] = Piece.Empty;
            }
            
            for (int i = 0; i < Constants.NUM_PLAYERS; i++)
            {
                bigPieces[i] = 0;
                majorPieces[i] = 0;
                minorPieces[i] = 0;
                material[i] = 0;
            }

            for (int i = 0; i < Constants.TOTAL_DIFF_PIECES; i++)
            {
                pieceNumbers[i] = None;
            }

            kingSquares[White] = kingSquares[Black] = (int) Squares120Enum.NO_SQ;
            sideToPlay = Both;
            enPassantSquare = (int) Squares120Enum.NO_SQ;
            fiftyMoveCounter = 0;
            ply = 0;
            histPly = 0;
            castlingRights = 0;
            positionKey = 0UL;
            pawns = new ulong[3] { 0UL, 0UL, 0UL } ;
        }

        private void initBitBoards()
        {
            for (int rank = 0; rank < Constants.NUM_RANKS; rank++)
            {
                for (int file = 0; file < Constants.NUM_FILES; file++)
                {
                    int square = frTo120Sq(file, rank);
                    if (squares[sq120(rank * 8 + file)] == Piece.WhitePawn)
                    {
                        pawns[White] |= (1UL << sq64(square));
                    } 
                    else if (squares[sq120(rank * 8 + file)] == Piece.BlackPawn)
                    {
                        pawns[Black] |= (1UL << sq64(square));
                    }
                }
            } 
        }

        private void initHashKeys()
        {
            System.Random rand = new System.Random();
            for (int i = 0; i < Constants.TOTAL_DIFF_PIECES; i++)
            {
                for (int j = 0; j < Constants.NUM_SQUARES_EXT; j++)
                {
                    pieceKeys[i, j] = Hashkey.rand64(rand);
                }
            }
            sideKey = Hashkey.rand64(rand);
            for (int i = 0; i < 16; i++)
            {
                castleKeys[i] = Hashkey.rand64(rand);
            }
        }

        private void updateListsMaterial()
        {
            int piece, square, color;
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++)
            {
                square = i;
                piece = squares[i];
                if (piece != Piece.Empty && piece != (int) Squares120Enum.OFFBOARD)
                {
                    color = Piece.PieceCol[piece];
                    if (Piece.PieceBig[piece]) bigPieces[color]++;
                    if (Piece.PieceMaj[piece]) majorPieces[color]++;
                    if (Piece.PieceMin[piece]) minorPieces[color]++;
                    material[color] += Piece.PieceVal[piece];
                    pieceList[piece,pieceNumbers[piece]] = square;
                    pieceNumbers[piece]++;

                    switch (piece)
                    {
                        case Piece.WhiteKing:
                            Assert.IsTrue(color == Piece.White, "Piece color set incorrectly in board");
                            kingSquares[color] = square;
                            break;
                        case Piece.BlackKing:
                            Assert.IsTrue(color == Piece.Black, "Piece color set incorrectly in board");
                            kingSquares[color] = square;
                            break;
                        case Piece.WhitePawn:
                            Assert.IsTrue(color == Piece.White, "Piece color set incorrectly in board");
                            setBit(ref pawns[White], sq64(square));
                            setBit(ref pawns[Both], sq64(square));
                            break;
                        case Piece.BlackPawn:
                            Assert.IsTrue(color == Piece.Black, "Piece color set incorrectly in board");
                            setBit(ref pawns[Black], sq64(square));
                            setBit(ref pawns[Both], sq64(square));
                            break;
                    }
                }
            }
        }

        public static int frTo120Sq(int file, int rank)
        {
            return (21 + file + rank * 10);
        }

        public static int sq64(int square)
        {
            return sq120ToSq64[square];
        }

        public static int sq120(int square)
        {
            return sq64ToSq120[square];
        }

        public void printGameBoard()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Game board");
            sb.AppendLine();
            for (int i = Constants.NUM_SQUARES - 1; i >= None; i--)
            {
                if ((i + 1) % 8 == 0)
                {
                    sb.AppendLine();
                }
                sb.Append(squares[sq120(i)].ToString());
                sb.Append("  ");
            }
            Debug.Log(sb.ToString());
        }

        public void print120Board()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("120 board");
            sb.AppendLine();
            for (int i = Constants.NUM_SQUARES_EXT - 1; i >= None; i--)
            {
                if ((i + 1) % 10 == 0)
                {
                    sb.AppendLine();
                }
                sb.Append(squares[i].ToString());
                sb.Append("  ");
            }
            Debug.Log(sb.ToString());
        }

        // Bitboard operations
        public void printBitBoard(ulong bitBoard, int player)
        {
            ulong shiftMe = 1UL;
            int square120 = 0;
            int square64 = 0;
            StringBuilder sb = new StringBuilder();
            if (player == White)
            {
                sb.Append("White BitBoard:");
            }
            else if (player == Black)
            {
                sb.Append("Black BitBoard:");
            }
            else if (player == Both)
            {
                sb.Append("Both BitBoard:");
            }
            else
            {
                sb.Append("Specific BitBoard:");
            }
            sb.AppendLine();
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file <= 7; file++)
                {
                    square120 = frTo120Sq(file, rank);
                    square64 = sq64(square120);
                    if (((shiftMe << square64) & bitBoard) != 0)
                        sb.Append("X");
                    else 
                        sb.Append("-");
                }

                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }

        private bool checkBoard()
        {
            // This function is a class invariant (Design by Contract). It creates "false" variables mirroring the real
            // Board class variables in order to check that they are correct at all times.
            int[] tPieceNumbers = new int[13] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            int[] tBigPieces = new int[2] { 0, 0};
            int[] tMajorPieces = new int[2] { 0, 0};
            int[] tMinorPieces = new int[2] { 0, 0};
            int[] tMaterial = new int[2] { 0, 0};
            int tPiece, color, sq64;
            ulong[] tPawns = new ulong[3] {0UL, 0UL, 0UL};
            tPawns[White] = pawns[White];
            tPawns[Black] = pawns[Black];
            tPawns[Both] = pawns[Both];

            // Check piece lists
            for (tPiece = Piece.WhitePawn; tPiece <= Piece.BlackKing; tPiece++)
            {
                for (int tPieceNumber = 0; tPieceNumber < pieceNumbers[tPiece]; tPieceNumber++)
                {
                    int sq120 = pieceList[tPiece, tPieceNumber];
                    Assert.IsTrue(squares[sq120] == tPiece, "checkBoard() failed: Piece found in Piece List was not found in the board.");
                }
            }
            for (sq64 = 0; sq64 < Constants.NUM_SQUARES; sq64++)
            {
                int sq120 = Board.sq120(sq64);
                tPiece = squares[sq120];
                if (tPiece != Piece.Empty)
                {
                    tPieceNumbers[tPiece]++;
                    color = Piece.PieceCol[tPiece];
                    if (Piece.PieceBig[tPiece]) tBigPieces[color]++;
                    if (Piece.PieceMaj[tPiece]) tMajorPieces[color]++;
                    if (Piece.PieceMin[tPiece]) tMinorPieces[color]++;
                    tMaterial[color] += Piece.PieceVal[tPiece];
                }
            }

            // Check piece numbers
            for (tPiece = Piece.WhitePawn; tPiece <= Piece.BlackKing; tPiece++)
            {
                Assert.IsTrue(tPieceNumbers[tPiece] == pieceNumbers[tPiece],"checkBoard() failed: pieceNumbers list set incorrectly.");
            }
            
            // Check BitBoards
            Assert.IsTrue(BitBoardUtils.countBitBoard(tPawns[White]) == pieceNumbers[Piece.WhitePawn], "checkBoard() failed: White pawns set incorrectly in BitBoard.");
            Assert.IsTrue(BitBoardUtils.countBitBoard(tPawns[Black]) == pieceNumbers[Piece.BlackPawn], "checkBoard() failed: Black pawns set incorrectly in BitBoard.");
            Assert.IsTrue(BitBoardUtils.countBitBoard(tPawns[Both]) == pieceNumbers[Piece.WhitePawn] + pieceNumbers[Piece.BlackPawn], "checkBoard() failed: Both pawns set incorrectly in BitBoard.");

            // Check BitBoards squares
            while (tPawns[White] != 0)
            {
                sq64 = BitBoardUtils.popBit(ref tPawns[White]);
                Assert.IsTrue(squares[sq120(sq64)] == Piece.WhitePawn, "checkBoard() failed: White pawn not found in BitBoard.");
            }
            while (tPawns[Black] != 0)
            {
                sq64 = BitBoardUtils.popBit(ref tPawns[Black]);
                Assert.IsTrue(squares[sq120(sq64)] == Piece.BlackPawn, "checkBoard() failed: Black pawn not found in BitBoard.");
            }
            while (tPawns[Both] != 0)
            {
                sq64 = BitBoardUtils.popBit(ref tPawns[Both]);
                Assert.IsTrue(squares[sq120(sq64)] == Piece.WhitePawn || squares[sq120(sq64)] == Piece.BlackPawn, "checkBoard() failed: Both pawn not found in BitBoard.");
            }
            
            // Various checks
            Assert.IsTrue(tMaterial[White] == material[White] && tMaterial[Black] == material[Black], "checkBoard() failed: Material count not equal.");
            Assert.IsTrue(tMinorPieces[White] == minorPieces[White] && tMinorPieces[Black] == minorPieces[Black], "checkBoard() failed: Minor pieces count not equal.");
            Assert.IsTrue(tMajorPieces[White] == majorPieces[White] && tMajorPieces[Black]== majorPieces[Black], "checkBoard() failed: Major pieces count not equal.");
            Assert.IsTrue(tBigPieces[White] == bigPieces[White] && tBigPieces[Black] == bigPieces[Black], "checkBoard() failed: Big pieces pieces count not equal.");
            Assert.IsTrue(sideToPlay == White || sideToPlay == Black, "checkBoard() failed: Side to play not set to White or Black.");
            Assert.IsTrue(generatePositionKey() == positionKey, "checkBoard() failed: Incorrect Position Key.");
            Assert.IsTrue(enPassantSquare == (int) Squares120Enum.NO_SQ || squareRank[enPassantSquare] == (int) Constants.RanksEnum.RANK_6 && sideToPlay == White
                                            || squareRank[enPassantSquare] == (int) Constants.RanksEnum.RANK_3 && sideToPlay == Black, "checkBoard() failed: Incorrect enPassant rank.");
            Assert.IsTrue(squares[kingSquares[White]] == Piece.WhiteKing, "checkBoard() failed: White King not found in expected square.");
            Assert.IsTrue(squares[kingSquares[Black]] == Piece.BlackKing, "checkBoard() failed: Black King not found in expected square.");
            
            return true;
        }
        
        // Bitboard operations
        private void clearBit(ref ulong bitBoard, int square)
        {
            bitBoard &= clearMask[square];
        }

        private void setBit(ref ulong bitBoard, int square)
        {
            bitBoard |= setMask[square];
        }
        
        // Position key
        public ulong generatePositionKey()
        {
            ulong finalKey = 0;
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++)
            {
                int piece = squares[i];
                if (piece != (int) Squares120Enum.NO_SQ && piece != Piece.Empty && piece != (int) Squares120Enum.OFFBOARD)
                {
                    Assert.IsTrue(piece >= Piece.WhitePawn && piece <= Piece.BlackKing);
                    finalKey ^= pieceKeys[piece,i];
                }
            }
            if (sideToPlay == White)
            {
                finalKey ^= sideKey;
            }
            if (enPassantSquare != (int) Squares120Enum.NO_SQ)
            {
                Assert.IsTrue(enPassantSquare >= 0 && enPassantSquare < Constants.NUM_SQUARES_EXT);
                finalKey ^= pieceKeys[Piece.Empty, enPassantSquare];
            }
            Assert.IsTrue(castlingRights >= 0 && castlingRights <= 15);
            finalKey ^= castleKeys[castlingRights];

            return finalKey;
        }
    }
}
