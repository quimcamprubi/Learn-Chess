using System;
using Utils;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine.Assertions;

namespace Core
{
    public class Board
    {
        // Game board
        public int[] squares;
        
        // Indices
        public static int[] sq64ToSq120 = new int[Constants.NUM_SQUARES];       // 64 indices to 120 indices
        public static int[] sq120ToSq64 = new int[Constants.NUM_SQUARES_EXT];   // 120 indices to 64 indices
        private int[] kingSquares = new int[Constants.NUM_KINGS];

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
        private int[] bigPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[] majorPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[] minorPieces = new int[Constants.PIECE_TYPE_VARIANTS];
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
            initBitBoards();
            initHashKeys();
            positionKey = generatePositionKey();
            printGameBoard();
            print120Board();
            printBitBoard(pawns[White], White);
            printBitBoard(pawns[Black], Black);
        }
        
        private void initBoards()
        {
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++)
            {
                sq120ToSq64[i] = 65;
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
        }

        private void resetBoard()
        {
            for (int i = 0; i < Constants.NUM_PIECE_VARIANTS; i++)
            {
                bigPieces[i] = 0;
                majorPieces[i] = 0;
                minorPieces[i] = 0;
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
