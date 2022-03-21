﻿using System;
using Utils;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;

namespace Core
{
    public class Board
    {
        // Game board
        public int[] squares;
        
        // Indices
        private int[] sq64ToSq120 = new int[Constants.NUM_SQUARES];
        private int[] sq120ToSq64 = new int[Constants.NUM_SQUARES_EXT];
        private int[] kingSquares = new int[Constants.NUM_KINGS];

        // Game rules
        private int sideToPlay;
        private int enPassantSquareInd;
        private int fiftyMoveCounter;
        private int ply;
        private int histPly;
        private int castlingRights;
        private Move[] gameHist = new Move[Constants.MAX_GAME_MOVES];

        // Position key
        private ulong positionKey;
        
        // Piece lists
        private int[] pieceNumbers = new int[Constants.TOTAL_DIFF_PIECES];
        private int[] bigPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[] majorPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[] minorPieces = new int[Constants.PIECE_TYPE_VARIANTS];
        private int[,] pieceList = new int[Constants.TOTAL_DIFF_PIECES, Constants.MAX_PIECES_OF_SAME_TYPE];

        // Bitboard
        private ulong[] pawns = new UInt64[Constants.NUM_PIECE_VARIANTS];
        private ulong[] setMask = new ulong[Constants.NUM_SQUARES];
        private ulong[] clearMask = new ulong[Constants.NUM_SQUARES];
        
        public static int NoPlayer = 0;
        public static int White = 1;
        public static int Black = 2;

        public enum Squares120Enum
        {
            A1 = 21, B1, C1, D1, E1, F1, G1, H1,
            A2 = 31, B2, C2, D2, E2, F2, G2, H2,
            A3 = 41, B3, C3, D3, E3, F3, G3, H3,
            A4 = 51, B4, C4, D4, E4, F4, G4, H4,
            A5 = 61, B5, C5, D5, E5, F5, G5, H5,
            A6 = 71, B6, C6, D6, E6, F6, G6, H6,
            A7 = 81, B7, C7, D7, E7, F7, G7, H7,
            A8 = 91, B8, C8, D8, E8, F8, G8, H8, NO_SQ
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
            ChessPosition loadedPosition = FenDecoder.DecodePositionFromFen(Constants.startingFen);
            squares = loadedPosition.squares;
            initBoards();
        }

        private void initBoards()
        {
            int square = (int) Squares120Enum.A1;
            int square64 = 0;
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

            for (int rank = 0; rank < Constants.NUM_RANKS; rank++)
            {
                for (int file = 0; file < Constants.NUM_FILES; file++)
                {
                    square = frTo120Sq(file, rank);
                    sq64ToSq120[square64] = square;
                    sq120ToSq64[square] = square64;
                    if (squares[rank * 8 + file] == Piece.WhitePawn)
                    {
                        pawns[White] |= (1UL << sq64From120(square));
                    } 
                    else if (squares[rank * 8 + file] == Piece.BlackPawn)
                    {
                        pawns[Black] |= (1UL << sq64From120(square));
                    }
                    square64++;
                }
            }
            printGameBoard();
            printBitBoard(pawns[White], White);
            printBitBoard(pawns[Black], Black);
        }

        public int frTo120Sq(int file, int rank)
        {
            return (21 + file + rank * 10);
        }

        public int sq64From120(int square)
        {
            return sq120ToSq64[square];
        }

        public void printGameBoard()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Game board");
            sb.AppendLine();
            for (int i = 0; i < Constants.NUM_SQUARES; i++)
            {
                if (i % 8 == 0)
                {
                    sb.AppendLine();
                }
                sb.Append(squares[i].ToString());
                sb.Append("  ");
            }
            Debug.Log(sb.ToString());
        }

        public void print120Board()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("120 board");
            sb.AppendLine();
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++)
            {
                if (i % 10 == 0)
                {
                    sb.AppendLine();
                }
                sb.Append(sq120ToSq64[i].ToString());
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
            sb.AppendLine();
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file <= 7; file++)
                {
                    square120 = frTo120Sq(file, rank);
                    square64 = sq64From120(square120);
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
    }
}
