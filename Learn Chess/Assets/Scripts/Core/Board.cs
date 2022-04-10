using System;
using Utils;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UI;
using UnityEditor;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Core {
    public class Board {
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
        public int ply;                                                        // Total half-moves played in current search
        public int histPly;                                                    // Total half-moves played in the game (and used to index gameHist)
        public int castlingRights;                                             // 4 bits for castling rights: WKCA WQCA BKCA BQCA
        private UndoMove[] gameHist = new UndoMove[Constants.MAX_GAME_MOVES].Populate(() => new UndoMove());

        // Hash key
        public ulong positionKey;
        private ulong[,] pieceKeys = new ulong[Constants.TOTAL_DIFF_PIECES, Constants.NUM_SQUARES_EXT];
        private ulong sideKey;
        private ulong[] castleKeys = new ulong[16];
        
        // Piece lists
        public int[] pieceNumbers = new int[Constants.TOTAL_DIFF_PIECES];
        public int[] bigPieces = new int[Constants.NUM_PLAYERS];
        public int[] majorPieces = new int[Constants.NUM_PLAYERS];
        public int[] minorPieces = new int[Constants.NUM_PLAYERS];
        public int[] material = new int[Constants.NUM_PLAYERS];
        public int[,] pieceList = new int[Constants.TOTAL_DIFF_PIECES, Constants.MAX_PIECES_OF_SAME_TYPE];

        // Bitboard
        private ulong[] pawns = new ulong[Constants.NUM_PIECE_VARIANTS];
        private ulong[] setMask = new ulong[Constants.NUM_SQUARES];
        private ulong[] clearMask = new ulong[Constants.NUM_SQUARES];
        
        public static int None = 0; // 0 constant
        public static int OFFBOARD = 100;
        
        public static int White = 0;
        public static int Black = 1;
        public static int Both = 2;

        public static int NoEnPassant = -1;

        public enum Squares120Enum {
            A1 = 21, B1, C1, D1, E1, F1, G1, H1,
            A2 = 31, B2, C2, D2, E2, F2, G2, H2,
            A3 = 41, B3, C3, D3, E3, F3, G3, H3,
            A4 = 51, B4, C4, D4, E4, F4, G4, H4,
            A5 = 61, B5, C5, D5, E5, F5, G5, H5,
            A6 = 71, B6, C6, D6, E6, F6, G6, H6,
            A7 = 81, B7, C7, D7, E7, F7, G7, H7,
            A8 = 91, B8, C8, D8, E8, F8, G8, H8, NO_SQ, OFFBOARD
        }

        public enum SquaresEnum {
            A1 = 0, B1, C1, D1, E1, F1, G1, H1,
            A2 = 8, B2, C2, D2, E2, F2, G2, H2,
            A3 = 16, B3, C3, D3, E3, F3, G3, H3,
            A4 = 24, B4, C4, D4, E4, F4, G4, H4,
            A5 = 32, B5, C5, D5, E5, F5, G5, H5,
            A6 = 40, B6, C6, D6, E6, F6, G6, H6,
            A7 = 48, B7, C7, D7, E7, F7, G7, H7,
            A8 = 56, B8, C8, D8, E8, F8, G8, H8, NO_SQ
        }

        public Board() {
            InitBoards();
        }

        public void LoadStartingPosition() {
            ResetBoard();
            InitFromPosition(FenDecoder.DecodePositionFromFen(Constants.startingFen));
            Assert.IsTrue(CheckBoard());
        }

        public void LoadPosition(ChessPosition position) {
            ResetBoard();
            InitFromPosition(position);
            Assert.IsTrue(CheckBoard());
            /*PrintGameBoard();
            Print120Board();
            PrintBitBoard(pawns[White], White);
            PrintBitBoard(pawns[Black], Black);
            PrintBitBoard(pawns[Both], Both);*/
        }
        
        // Board initialization
        private void InitBoards() {
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++) {
                sq120ToSq64[i] = 65;
                squareFile[i] = OFFBOARD;
                squareRank[i] = OFFBOARD;
            }
            for (int i = 0; i < Constants.NUM_SQUARES; i++) {
                sq64ToSq120[i] = 120;
                setMask[i] = 0UL;
                clearMask[i] = 0UL;
            }
            for (int i = 0; i < Constants.NUM_SQUARES; i++) {
                setMask[i] |= (1UL << i);
                clearMask[i] = ~setMask[i];
            }
            int square64 = 0;
            for (int rank = 0; rank < Constants.NUM_RANKS; rank++) {
                for (int file = 0; file < Constants.NUM_FILES; file++) {
                    int square = FrTo120Sq(file, rank);
                    sq64ToSq120[square64] = square;
                    sq120ToSq64[square] = square64;
                    squareFile[square] = file;
                    squareRank[square] = rank;
                    square64++;
                }
            }
        }

        private void InitFromPosition(ChessPosition loadedPosition) {
            squares = loadedPosition.squares;
            sideToPlay = loadedPosition.whiteToMove ? White : Black;
            castlingRights = loadedPosition.castlingRights;
            enPassantSquare = loadedPosition.enPassantSquare;
            ply = loadedPosition.plyCount;
            InitHashKeys();
            positionKey = GeneratePositionKey();
            UpdateListsMaterial();
        }

        private void ResetBoard() {
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++) {
                squares[i] = OFFBOARD;
            }
            for (int i = 0; i < Constants.NUM_SQUARES; i++) {
                squares[Sq120(i)] = Piece.Empty;
            }
            for (int i = 0; i < Constants.NUM_PLAYERS; i++) {
                bigPieces[i] = 0;
                majorPieces[i] = 0;
                minorPieces[i] = 0;
                material[i] = 0;
            }
            pawns = new ulong[3] { 0UL, 0UL, 0UL } ;
            for (int i = 0; i < Constants.TOTAL_DIFF_PIECES; i++) {
                pieceNumbers[i] = None;
            }
            kingSquares[White] = kingSquares[Black] = (int) Squares120Enum.NO_SQ;
            sideToPlay = Both;
            enPassantSquare = NoEnPassant;
            fiftyMoveCounter = 0;
            ply = 0;
            histPly = 0;
            castlingRights = 0;
            positionKey = 0UL;
        }

        private void InitBitBoards() {
            for (int rank = 0; rank < Constants.NUM_RANKS; rank++) {
                for (int file = 0; file < Constants.NUM_FILES; file++) {
                    int square = FrTo120Sq(file, rank);
                    if (squares[Sq120(rank * 8 + file)] == Piece.WhitePawn) {
                        pawns[White] |= (1UL << Sq64(square));
                    } else if (squares[Sq120(rank * 8 + file)] == Piece.BlackPawn) {
                        pawns[Black] |= (1UL << Sq64(square));
                    }
                }
            } 
        }

        private void InitHashKeys() {
            System.Random rand = new System.Random();
            for (int i = 0; i < Constants.TOTAL_DIFF_PIECES; i++) {
                for (int j = 0; j < Constants.NUM_SQUARES_EXT; j++) {
                    pieceKeys[i, j] = Hashkey.Rand64(rand);
                }
            }
            sideKey = Hashkey.Rand64(rand);
            for (int i = 0; i < 16; i++) {
                castleKeys[i] = Hashkey.Rand64(rand);
            }
        }

        // Update data
        private void UpdateListsMaterial() {
            int piece, square, color;
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++) {
                square = i;
                piece = squares[i];
                if (piece != Piece.Empty && piece != OFFBOARD) {
                    color = Piece.PieceColor[piece];
                    if (Piece.PieceBig[piece]) bigPieces[color]++;
                    if (Piece.PieceMaj[piece]) majorPieces[color]++;
                    if (Piece.PieceMin[piece]) minorPieces[color]++;
                    material[color] += Piece.PieceVal[piece];
                    pieceList[piece,pieceNumbers[piece]] = square;
                    pieceNumbers[piece]++;

                    switch (piece) {
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
                            SetBit(ref pawns[White], Sq64(square));
                            SetBit(ref pawns[Both], Sq64(square));
                            break;
                        case Piece.BlackPawn:
                            Assert.IsTrue(color == Piece.Black, "Piece color set incorrectly in board");
                            SetBit(ref pawns[Black], Sq64(square));
                            SetBit(ref pawns[Both], Sq64(square));
                            break;
                    }
                }
            }
        }
        
        public bool CheckBoard() {
            // This function is a class invariant (Design by Contract). It creates "false" variables mirroring the real
            // Board class variables in order to check that they are correct at all times.
            //Debug.Log("In checkboard");
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
            for (tPiece = Piece.WhitePawn; tPiece <= Piece.BlackKing; tPiece++) {
                for (int tPieceNumber = 0; tPieceNumber < pieceNumbers[tPiece]; tPieceNumber++) {
                    int sq120 = pieceList[tPiece, tPieceNumber];
                    Assert.IsTrue(squares[sq120] == tPiece, "checkBoard() failed: Piece found in Piece List was not found in the board.");
                }
            }
            for (sq64 = 0; sq64 < Constants.NUM_SQUARES; sq64++) {
                int sq120 = Sq120(sq64);
                tPiece = squares[sq120];
                if (tPiece != Piece.Empty) {
                    tPieceNumbers[tPiece]++;
                    color = Piece.PieceColor[tPiece];
                    if (Piece.PieceBig[tPiece]) tBigPieces[color]++;
                    if (Piece.PieceMaj[tPiece]) tMajorPieces[color]++;
                    if (Piece.PieceMin[tPiece]) tMinorPieces[color]++;
                    tMaterial[color] += Piece.PieceVal[tPiece];
                }
            }

            // Check piece numbers
            for (tPiece = Piece.WhitePawn; tPiece <= Piece.BlackKing; tPiece++) {
                Assert.IsTrue(tPieceNumbers[tPiece] == pieceNumbers[tPiece],"checkBoard() failed: pieceNumbers list set incorrectly.");
            }
            
            // Check BitBoards
            Assert.IsTrue(BitBoardUtils.CountBitBoard(tPawns[White]) == pieceNumbers[Piece.WhitePawn], "checkBoard() failed: White pawns set incorrectly in BitBoard.");
            Assert.IsTrue(BitBoardUtils.CountBitBoard(tPawns[Black]) == pieceNumbers[Piece.BlackPawn], "checkBoard() failed: Black pawns set incorrectly in BitBoard.");
            Assert.IsTrue(BitBoardUtils.CountBitBoard(tPawns[Both]) == pieceNumbers[Piece.WhitePawn] + pieceNumbers[Piece.BlackPawn], "checkBoard() failed: Both pawns set incorrectly in BitBoard.");

            // Check BitBoards squares
            while (tPawns[White] != 0) {
                sq64 = BitBoardUtils.PopBit(ref tPawns[White]);
                Assert.IsTrue(squares[Sq120(sq64)] == Piece.WhitePawn, "checkBoard() failed: White pawn not found in BitBoard.");
            }
            while (tPawns[Black] != 0) {
                sq64 = BitBoardUtils.PopBit(ref tPawns[Black]);
                Assert.IsTrue(squares[Sq120(sq64)] == Piece.BlackPawn, "checkBoard() failed: Black pawn not found in BitBoard.");
            }
            while (tPawns[Both] != 0) {
                sq64 = BitBoardUtils.PopBit(ref tPawns[Both]);
                Assert.IsTrue(squares[Sq120(sq64)] == Piece.WhitePawn || squares[Sq120(sq64)] == Piece.BlackPawn, "checkBoard() failed: Both pawn not found in BitBoard.");
            }
            
            // Various checks
            Assert.IsTrue(tMaterial[White] == material[White] && tMaterial[Black] == material[Black], "checkBoard() failed: Material count not equal.");
            Assert.IsTrue(tMinorPieces[White] == minorPieces[White] && tMinorPieces[Black] == minorPieces[Black], "checkBoard() failed: Minor pieces count not equal.");
            Assert.IsTrue(tMajorPieces[White] == majorPieces[White] && tMajorPieces[Black]== majorPieces[Black], "checkBoard() failed: Major pieces count not equal.");
            Assert.IsTrue(tBigPieces[White] == bigPieces[White] && tBigPieces[Black] == bigPieces[Black], "checkBoard() failed: Big pieces pieces count not equal.");
            Assert.IsTrue(sideToPlay == White || sideToPlay == Black, "checkBoard() failed: Side to play not set to White or Black.");
            Assert.IsTrue(GeneratePositionKey() == positionKey, "checkBoard() failed: Incorrect Position Key.");
            Assert.IsTrue(enPassantSquare == NoEnPassant || squareRank[enPassantSquare] == (int) Constants.RanksEnum.RANK_6 && sideToPlay == White
                                            || squareRank[enPassantSquare] == (int) Constants.RanksEnum.RANK_3 && sideToPlay == Black, "checkBoard() failed: Incorrect enPassant rank.");
            Assert.IsTrue(squares[kingSquares[White]] == Piece.WhiteKing, "checkBoard() failed: White King not found in expected square.");
            Assert.IsTrue(squares[kingSquares[Black]] == Piece.BlackKing, "checkBoard() failed: Black King not found in expected square.");
            
            return true;
        }

        // Print various boards to Unity console
        public void PrintGameBoard() {
            StringBuilder sb = new StringBuilder();
            sb.Append("Game board");
            sb.AppendLine();
            List<string> rowList = new List<String>();
            for (int i = Constants.NUM_SQUARES - 1; i >= None; i--) {
                if ((i + 1) % 8 == 0) {
                    rowList.Reverse();
                    sb.AppendLine(String.Join("  ", rowList));
                    rowList = new List<String>();
                }
                rowList.Add(squares[Sq120(i)].ToString());
            }
            rowList.Reverse();
            sb.AppendLine(String.Join("  ", rowList));
            Debug.Log(sb.ToString());
        }

        public void Print120Board() {
            StringBuilder sb = new StringBuilder();
            sb.Append("120 board");
            sb.AppendLine();
            List<string> rowList = new List<String>();
            for (int i = Constants.NUM_SQUARES_EXT - 1; i >= None; i--) {
                if ((i + 1) % 10 == 0) {
                    rowList.Reverse();
                    sb.AppendLine(String.Join("  ", rowList));
                    rowList = new List<String>();
                }
                rowList.Add(squares[i].ToString());
            }
            Debug.Log(sb.ToString());
        }

        // Bitboard operations
        public void PrintBitBoard(ulong bitBoard, int player) {
            ulong shiftMe = 1UL;
            int square120 = 0;
            int square64 = 0;
            StringBuilder sb = new StringBuilder();
            if (player == White) {
                sb.Append("White BitBoard:");
            } else if (player == Black) {
                sb.Append("Black BitBoard:");
            } else if (player == Both) {
                sb.Append("Both BitBoard:");
            } else {
                sb.Append("Specific BitBoard:");
            }
            sb.AppendLine();
            for (int rank = 7; rank >= 0; rank--) {
                for (int file = 0; file <= 7; file++) {
                    square120 = FrTo120Sq(file, rank);
                    square64 = Sq64(square120);
                    if (((shiftMe << square64) & bitBoard) != 0) {
                        sb.Append("X");
                    }
                    else {
                        sb.Append("-");
                    }
                }
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }

        // Bitboard operations
        private void ClearBit(ref ulong bitBoard, int square) {
            bitBoard &= clearMask[square];
        }

        private void SetBit(ref ulong bitBoard, int square) {
            bitBoard |= setMask[square];
        }
        
        // Position key
        public ulong GeneratePositionKey() {
            ulong finalKey = 0;
            for (int i = 0; i < Constants.NUM_SQUARES_EXT; i++) {
                int piece = squares[i];
                if (piece != (int) Squares120Enum.NO_SQ && piece != Piece.Empty && piece != OFFBOARD) {
                    Assert.IsTrue(piece >= Piece.WhitePawn && piece <= Piece.BlackKing);
                    finalKey ^= pieceKeys[piece,i];
                }
            }
            if (sideToPlay == White) {
                finalKey ^= sideKey;
            }
            if (enPassantSquare != NoEnPassant) {
                Assert.IsTrue(enPassantSquare >= 0 && enPassantSquare < Constants.NUM_SQUARES_EXT);
                finalKey ^= pieceKeys[Piece.Empty, enPassantSquare];
            }
            Assert.IsTrue(castlingRights >= 0 && castlingRights <= 15);
            finalKey ^= castleKeys[castlingRights];
            return finalKey;
        }
        
        // Is Square attacked?
        public bool IsSquareAttacked(int square, int attackingSide) {
            Assert.IsTrue(Validations.IsSquareOnBoard(square), "Invalid square attacked index.");
            Assert.IsTrue(Validations.IsSideValid(attackingSide), "Incorrect attackign side.");
            Assert.IsTrue(CheckBoard(), "CheckBoard validation incorrect at IsSquareAttacked()");
            // Check if a Pawn is attacking the square
            if (attackingSide == White) {
                if (squares[square - 11] == Piece.WhitePawn || squares[square - 9] == Piece.WhitePawn) {
                    return true;
                }
            } else {
                if (squares[square + 11] == Piece.BlackPawn || squares[square + 9] == Piece.BlackPawn) {
                    return true;
                }
            }
            
            // Check if a Knight is attacking the square
            for (int i = 0; i < 8; i++) {
                int piece = squares[square + Directions.KnightDirections[i]];
                if (piece != OFFBOARD) {
                    if (IsPieceKnight(piece) && Piece.PieceColor[piece] == attackingSide) {
                        return true;
                    }
                }
            }
            
            // Check if a Rook or a Queen is attacking the square
            for (int i = 0; i < 4; i++) {
                int direction = Directions.RookDirections[i];
                int tSquare = square + direction;
                int piece = squares[tSquare];
                // Because Rooks and Queens are sliding pieces, we keep going until we run into the edge of the board, or into a piece.
                while (piece != OFFBOARD) {
                    if (piece != None) {
                        if (IsPieceRookQueen(piece) && Piece.PieceColor[piece] == attackingSide) {
                            return true;
                        }
                        break;
                    }
                    tSquare += direction;
                    piece = squares[tSquare];
                }
            }
            
            // Check if a Bishop or a Queen is attacking the square
            for (int i = 0; i < 4; i++) {
                int direction = Directions.BishopDirections[i];
                int tSquare = square + direction;
                int piece = squares[tSquare];
                // Because Bishops and Queens are sliding pieces, we keep going until we run into the edge of the board, or into a piece.
                while (piece != OFFBOARD) {
                    if (piece != None) {
                        if (IsPieceBishopQueen(piece) && Piece.PieceColor[piece] == attackingSide) {
                            return true;
                        }
                        break;
                    }
                    tSquare += direction;
                    piece = squares[tSquare];
                }
            }
            
            // Check if a King is attacking the square
            for (int i = 0; i < 8; i++) {
                int piece = squares[square + Directions.KingDirections[i]];
                if (piece != OFFBOARD) {
                    if (IsPieceKing(piece) && Piece.PieceColor[piece] == attackingSide) {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public void ShowSquaresAttackedBySide(int side) {
            StringBuilder sb = new StringBuilder();
            if (side == White) {
                sb.Append("White squares attacked:");
            } else if (side == Black) {
                sb.Append("Black squares attacked:");
            } else if (side == Both) {
                sb.Append("Both squares attacked:");
            } else {
                sb.Append("Specific squares attacked:");
            }
            sb.AppendLine();
            for (int rank = 7; rank >= 0; rank--) {
                for (int file = 0; file <= 7; file++) {
                    int square = FrTo120Sq(file, rank);
                    sb.Append(IsSquareAttacked(square, side) ? "X" : "-");
                }
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }

        public bool MakeMove(Move move) { // If after making the move, the playing side King is in check, we will return false (illegal move).
            // If the playing side King is not in check after the move, the move is legal. 
            Assert.IsTrue(CheckBoard());
            int fromSquare = Move.FromSquare(move.move);
            int toSquare = Move.ToSquare(move.move);
            int side = sideToPlay;
            Assert.IsTrue(Validations.IsSquareOnBoard(fromSquare), "From Square for MakeMove() not on board");
            Assert.IsTrue(Validations.IsSquareOnBoard(toSquare), "To Square for MakeMove() not on board");
            Assert.IsTrue(Validations.IsSideValid(side));
            Assert.IsTrue(Validations.IsPieceValid(squares[fromSquare]));
            gameHist[histPly].positionKey = positionKey;
            if (Move.IsEnPassantCapture(move.move)) {
                if (side == White) {
                    ClearPiece(toSquare - Directions.PawnForward);
                } else {
                    ClearPiece(toSquare + Directions.PawnForward);
                }
            } else if (Move.IsCastlingMove(move.move)) {
                switch (toSquare) {
                    case (int) Squares120Enum.C1:
                        MovePiece((int) Squares120Enum.A1, (int) Squares120Enum.D1);
                        break;
                    case (int) Squares120Enum.G1:
                        MovePiece((int) Squares120Enum.H1, (int) Squares120Enum.F1);
                        break;

                    case (int) Squares120Enum.C8:
                        MovePiece((int) Squares120Enum.A8, (int) Squares120Enum.D8);
                        break;
                    case (int) Squares120Enum.G8:
                        MovePiece((int) Squares120Enum.H8, (int) Squares120Enum.F8);
                        break;
                    default: Assert.IsTrue(false, "Castling move target square is wrong.");
                        break;
                }
            }
            
            if (enPassantSquare != NoEnPassant) HashEnPassant();
            HashCastling(); // Hash out the current state
            gameHist[histPly].move = move.move;
            gameHist[histPly].fiftyMove = fiftyMoveCounter;
            gameHist[histPly].enPassant = enPassantSquare;
            gameHist[histPly].castlingRights = castlingRights;
            castlingRights &= CastlingRights.CastlePerm[fromSquare];
            castlingRights &= CastlingRights.CastlePerm[toSquare];
            enPassantSquare = NoEnPassant;
            HashCastling(); // Hash the new castling rights

            int capturedPiece = Move.CapturedPiece(move.move);
            fiftyMoveCounter++;
            if (capturedPiece != Piece.Empty && !Move.IsEnPassantCapture(move.move)) {
                Assert.IsTrue(Validations.IsPieceValid(capturedPiece), "Captured piece in MakeMove() is not valid.");
                ClearPiece(toSquare);
                fiftyMoveCounter = 0;
            }
            histPly++;
            ply++;
            
            if (Piece.IsPiecePawn(squares[fromSquare])) {
                fiftyMoveCounter = 0;
                if (Move.IsPawnStartMove(move.move)) {
                    if (side == White) {
                        enPassantSquare = fromSquare + Directions.PawnForward;
                        Assert.IsTrue(squareRank[fromSquare + Directions.PawnForward] == (int) Constants.RanksEnum.RANK_3, "En Passant square being set is invalid.");
                    } else {
                        enPassantSquare = fromSquare - Directions.PawnForward;
                        Assert.IsTrue(squareRank[fromSquare - Directions.PawnForward] == (int) Constants.RanksEnum.RANK_6, "En Passant square being set is invalid.");
                    }
                    HashEnPassant();
                }
            }
            MovePiece(fromSquare, toSquare);
            int promotedPiece = Move.PromotedPiece(move.move);
            if (promotedPiece != Piece.Empty) {
                Assert.IsTrue(Validations.IsPieceValid(promotedPiece) && !Piece.IsPiecePawn(promotedPiece), "Promoted piece is invalid.");
                ClearPiece(toSquare);
                AddPiece(toSquare, promotedPiece);
            }
            if (Piece.IsPieceKing[squares[toSquare]]) {
                kingSquares[side] = toSquare;
            }
            sideToPlay ^= 1;
            HashSide();
            Assert.IsTrue(CheckBoard());
            // We must now check if the King is in check after the move. In case it is not, the move is illegal.
            if (IsSquareAttacked(kingSquares[side], sideToPlay)) { // If opposing player is attacking our King, move is legal.
                UnmakeMove();
                return false;
            }
            // Otherwise, the move is legal.
            return true;
        }

        public void UnmakeMove() { // This function reverts the last played move stored in our game history.
            Assert.IsTrue(CheckBoard());
            histPly--;
            ply--;
            int move = gameHist[histPly].move; // We retrieve the last move that was played.
            int fromSquare = Move.FromSquare(move);
            int toSquare = Move.ToSquare(move);
            Assert.IsTrue(Validations.IsSquareOnBoard(fromSquare), "From Square for UnmakeMove() not on board");
            Assert.IsTrue(Validations.IsSquareOnBoard(toSquare), "To Square for UnmakeMove() not on board");
            
            if (enPassantSquare != NoEnPassant) HashEnPassant();
            HashCastling(); // Hash out the current state
            castlingRights = gameHist[histPly].castlingRights;
            fiftyMoveCounter = gameHist[histPly].fiftyMove;
            enPassantSquare = gameHist[histPly].enPassant;
            if (enPassantSquare != NoEnPassant) HashEnPassant();
            HashCastling(); // Hash the new castling rights
            sideToPlay ^= 1;
            HashSide();

            if (Move.IsEnPassantCapture(move)) {
                if (sideToPlay == White) {
                    AddPiece(toSquare - Directions.PawnForward, Piece.BlackPawn);
                } else {
                    AddPiece(toSquare + Directions.PawnForward, Piece.WhitePawn);
                }
            } else if (Move.IsCastlingMove(move)) {
                switch (toSquare) {
                    case (int) Squares120Enum.C1:
                        MovePiece((int) Squares120Enum.D1, (int) Squares120Enum.A1);
                        break;
                    case (int) Squares120Enum.G1:
                        MovePiece((int) Squares120Enum.F1, (int) Squares120Enum.H1);
                        break;
                    case (int) Squares120Enum.C8:
                        MovePiece((int) Squares120Enum.D8, (int) Squares120Enum.A8);
                        break;
                    case (int) Squares120Enum.G8:
                        MovePiece((int) Squares120Enum.F8, (int) Squares120Enum.H8);
                        break;
                    default: Assert.IsTrue(false, "Castling move target square is wrong.");
                        break;
                }
            }
            MovePiece(toSquare, fromSquare);
            if (Piece.IsPieceKing[squares[fromSquare]]) {
                kingSquares[sideToPlay] = fromSquare;
            }
            int capturedPiece = Move.CapturedPiece(move);
            if (capturedPiece != Piece.Empty && !Move.IsEnPassantCapture(move)) {
                Assert.IsTrue(Validations.IsPieceValid(capturedPiece), "Captured piece in UnmakeMove() is not valid.");
                AddPiece(toSquare, capturedPiece);
            }
            int promotedPiece = Move.PromotedPiece(move);
            if (promotedPiece != Piece.Empty) {
                Assert.IsTrue(Validations.IsPieceValid(promotedPiece) && !Piece.IsPiecePawn(promotedPiece), "Promoted piece is invalid.");
                ClearPiece(fromSquare);
                AddPiece(fromSquare, sideToPlay == White ? Piece.WhitePawn : Piece.BlackPawn);
            }
            Assert.IsTrue(CheckBoard());
        }
        
        // MakeMove sub-functions
        public void ClearPiece(int square) {
            Assert.IsTrue(Validations.IsSquareOnBoard(square), "Square to clear is not on board");
            int piece = squares[square];
            int color = Piece.PieceColor[piece];
            int tPieceNumber = -1;
            HashPiece(piece, square);
            squares[square] = Piece.Empty;
            material[color] -= Piece.PieceVal[piece];
            if (Piece.PieceBig[piece]) {
                bigPieces[color]--;
                if (Piece.PieceMaj[piece]) {
                    majorPieces[color]--;
                } else {
                    minorPieces[color]--;
                }
            } else { // Piece is pawn
                ClearBit(ref pawns[color], Sq64(square));
                ClearBit(ref pawns[Both], Sq64(square));
            }
            for (int i = 0; i < pieceNumbers[piece]; i++) {
                if (pieceList[piece, i] == square) {
                    tPieceNumber = i;
                    break;
                }
            }
            Assert.IsTrue(tPieceNumber != -1, "Piece to clear not found in Piece List.");
            pieceNumbers[piece]--;
            pieceList[piece, tPieceNumber] = pieceList[piece, pieceNumbers[piece]]; 
            // We move the last piece in the pieceList to the cleared position and we update the number of pieces.
            // The last piece in pieceList is never checked again (even though it's still there) as our pieceNumbers goes one position lower.
        }

        public void AddPiece(int square, int piece) {
            Assert.IsTrue(Validations.IsPieceValid(piece), "Piece to add is invalid.");
            Assert.IsTrue(Validations.IsSquareOnBoard(square), "Square to add piece on is not on board");
            int color = Piece.PieceColor[piece];
            HashPiece(piece, square);
            squares[square] = piece;
            if (Piece.PieceBig[piece]) {
                bigPieces[color]++;
                if (Piece.PieceMaj[piece]) {
                    majorPieces[color]++;
                } else {
                    minorPieces[color]++;
                }
            } else { // Piece is pawn
                SetBit(ref pawns[color], Sq64(square));
                SetBit(ref pawns[Both], Sq64(square));
            }
            material[color] += Piece.PieceVal[piece];
            pieceList[piece, pieceNumbers[piece]++] = square;
        }

        public void MovePiece(int fromSquare, int toSquare) {
            Assert.IsTrue(Validations.IsSquareOnBoard(fromSquare), "From Square is not on board in MovePiece()");
            Assert.IsTrue(Validations.IsSquareOnBoard(toSquare), "To Square is not on board in MovePiece()");
            int piece = squares[fromSquare];
            int color = Piece.PieceColor[piece];
            HashPiece(piece, fromSquare);
            squares[fromSquare] = Piece.Empty;
            HashPiece(piece, toSquare);
            squares[toSquare] = piece;
            if (!Piece.PieceBig[piece]) { // If it's a pawn, we update the BitBoards
                ClearBit(ref pawns[color], Sq64(fromSquare));
                ClearBit(ref pawns[Both], Sq64(fromSquare));
                SetBit(ref pawns[color], Sq64(toSquare));
                SetBit(ref pawns[Both], Sq64(toSquare));
            }
            for (int i = 0; i < pieceNumbers[piece]; i++)
            {
                if (pieceList[piece, i] == fromSquare) {
                    pieceList[piece, i] = toSquare;
                    break;
                }
            }
        }
        
        public List<Move> GetLegalMovesForSquare(int square, List<Move> currentPseudoLegalMoves) {
            List<Move> pieceMoves = currentPseudoLegalMoves.Where(move => Move.FromSquare(move.move) == square).ToList();
            List<Move> legalMoves = new List<Move>();
            foreach (Move pseudoLegalMove in pieceMoves) {
                if (MakeMove(pseudoLegalMove)) {
                    UnmakeMove();
                    legalMoves.Add(pseudoLegalMove);
                }
            }
            return legalMoves;
        }

        public List<Move> GetLegalMovesInPosition(List<Move> currentPseudoLegalMoves) {
            List<Move> legalMoves = new List<Move>();
            foreach (Move pseudoLegalMove in currentPseudoLegalMoves) {
                //Debug.Log("Checking move legality" + Move.GetMoveString(pseudoLegalMove));
                if (MakeMove(pseudoLegalMove)) {
                    UnmakeMove();
                    legalMoves.Add(pseudoLegalMove);
                }
            }
            return legalMoves;
        }

        // Index conversions
        public static int FrTo120Sq(int file, int rank) { return (21 + file + rank * 10); }
        public static int Sq64(int square) { return sq120ToSq64[square]; }
        public static int Sq120(int square) { return sq64ToSq120[square]; }
        
        // Piece checkers
        public static bool IsPieceKnight(int piece) { return Piece.IsPieceKnight[piece]; }
        public static bool IsPieceKing(int piece) { return Piece.IsPieceKing[piece]; }
        public static bool IsPieceRookQueen(int piece) { return Piece.IsPieceRookQueen[piece]; }
        public static bool IsPieceBishopQueen(int piece) { return Piece.IsPieceBishopQueen[piece]; }
        
        public void HashPiece(int piece, int square) { positionKey ^= pieceKeys[piece, square]; }
        public void HashCastling() { positionKey ^= castleKeys[castlingRights]; }
        public void HashSide() { positionKey ^= sideKey; }
        public void HashEnPassant() { positionKey ^= pieceKeys[Piece.Empty, enPassantSquare]; }

        public bool IsKingInCheck() {
            if (IsSquareAttacked(kingSquares[sideToPlay], sideToPlay ^ 1)) {
                return true;
            }
            return false;
        }
    }
}
