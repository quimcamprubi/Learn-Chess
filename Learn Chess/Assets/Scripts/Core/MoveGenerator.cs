using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Core {
    public static class MoveGenerator {
        public static List<Move> GenerateMoves(Board board) {
            return new List<Move>();
        }

        public static void AddQuietMove(Board board, int moveValue, List<Move> moveList) {
            moveList.Add(new Move(moveValue));
        }
        public static void AddCaptureMove(Board board, int moveValue, List<Move> moveList) {
            moveList.Add(new Move(moveValue));
        }
        public static void AddEnPassantMove(Board board, int moveValue, List<Move> moveList) {
            moveList.Add(new Move(moveValue));
        }

        // White pieces
        public static void AddWhitePawnCaptureMove(int fromSquare, int toSquare, int capture, List<Move> moveList) {
            if (Board.squareRank[fromSquare] == (int) Constants.RanksEnum.RANK_7) { // Promotion
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.WhiteQueen));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.WhiteRook));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.WhiteBishop));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.WhiteKnight));
            } else {
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.Empty));
            }
        }
        public static void AddWhitePawnMove(int fromSquare, int toSquare, List<Move> moveList) {
            if (Board.squareRank[fromSquare] == (int) Constants.RanksEnum.RANK_7) { // Promotion
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.WhiteQueen));
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.WhiteRook));
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.WhiteBishop));
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.WhiteKnight));
            } else {
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.Empty));
            }
        }
        
        // Black pieces
        public static void AddBlackPawnCaptureMove(int fromSquare, int toSquare, int capture, List<Move> moveList) {
            if (Board.squareRank[fromSquare] == (int) Constants.RanksEnum.RANK_2) { // Promotion
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.BlackQueen));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.BlackRook));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.BlackBishop));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.BlackKnight));
            } else {
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.Empty));
            }
        }
        public static void AddBlackPawnMove(int fromSquare, int toSquare, List<Move> moveList) {
            if (Board.squareRank[fromSquare] == (int) Constants.RanksEnum.RANK_2) { // Promotion
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.BlackQueen));
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.BlackRook));
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.BlackBishop));
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.BlackKnight));
            } else {
                moveList.Add(new Move(fromSquare, toSquare, Piece.Empty, Piece.Empty));
            }
        }
        
        // Generate all moves in a given position
        public static void GenerateAllMoves(Board board, List<Move> moveList) {
            board.CheckBoard();
            if (board.sideToPlay == Board.White) {
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[Piece.WhitePawn]; pieceNumber++) {
                    int square = board.pieceList[Piece.WhitePawn, pieceNumber];
                    Assert.IsTrue(Validations.IsSquareOnBoard(square));
                    if (board.squares[square + Directions.PawnForward] == Piece.Empty) { // Forward move and double forward move
                        AddWhitePawnMove(square, square + Directions.PawnForward, moveList);
                        if (Board.squareRank[square] == (int) Constants.RanksEnum.RANK_2 &&
                            board.squares[square + Directions.PawnDoubleForward] == Piece.Empty) {
                            moveList.Add(new Move(square, square + Directions.PawnDoubleForward, 
                                Piece.Empty, Piece.Empty, pawnStart: true));
                        }
                    }
                    int squareAttackLeft = square + Directions.PawnCapLeft;
                    int squareAttackRight = square + Directions.PawnCapRight;
                    if (Validations.IsSquareOnBoard(squareAttackLeft) && Piece.PieceColor[board.squares[squareAttackLeft]] == Piece.Black) { // Capture left
                        AddWhitePawnCaptureMove(square, squareAttackLeft, board.squares[squareAttackLeft], moveList);
                    }
                    if (Validations.IsSquareOnBoard(squareAttackRight) && Piece.PieceColor[board.squares[squareAttackRight]] == Piece.Black) { // Capture right
                        AddWhitePawnCaptureMove(square, squareAttackRight, board.squares[squareAttackRight], moveList);
                    }
                    if (squareAttackLeft == board.enPassantSquare) {
                        Assert.IsTrue(board.squares[squareAttackLeft - Directions.PawnForward] == Piece.BlackPawn, "Attempt to make an En Passant capture on a piece that isn't a Black Pawn.");
                        moveList.Add(new Move(square, squareAttackLeft, Piece.BlackPawn,Piece.Empty, enPassantCapture: true));
                    }
                    if (squareAttackRight == board.enPassantSquare) {
                        Assert.IsTrue(board.squares[squareAttackRight - Directions.PawnForward] == Piece.BlackPawn, "Attempt to make an En Passant capture on a piece that isn't a Black Pawn.");
                        moveList.Add(new Move(square, squareAttackRight, Piece.BlackPawn, Piece.Empty, enPassantCapture: true));
                    }
                }
            }
            else { // Black pieces
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[Piece.BlackPawn]; pieceNumber++) {
                    int square = board.pieceList[Piece.BlackPawn, pieceNumber];
                    Assert.IsTrue(Validations.IsSquareOnBoard(square));
                    if (board.squares[square - Directions.PawnForward] == Piece.Empty) { // Forward move and double forward move
                        AddBlackPawnMove(square, square - Directions.PawnForward, moveList);
                        if (Board.squareRank[square] == (int) Constants.RanksEnum.RANK_7 &&
                            board.squares[square - Directions.PawnDoubleForward] == Piece.Empty) {
                            moveList.Add(new Move(square, square - Directions.PawnDoubleForward, 
                                Piece.Empty, Piece.Empty, pawnStart: true));
                        }
                    }
                    int squareAttackLeft = square - Directions.PawnCapLeft;
                    int squareAttackRight = square - Directions.PawnCapRight;
                    if (Validations.IsSquareOnBoard(squareAttackLeft) && Piece.PieceColor[board.squares[squareAttackLeft]] == Piece.White) { // Capture left
                        AddBlackPawnCaptureMove(square, squareAttackLeft, board.squares[squareAttackLeft], moveList);
                    }
                    if (Validations.IsSquareOnBoard(squareAttackRight) && Piece.PieceColor[board.squares[squareAttackRight]] == Piece.White) { // Capture right
                        AddBlackPawnCaptureMove(square, squareAttackRight, board.squares[squareAttackRight], moveList);
                    }
                    if (squareAttackLeft == board.enPassantSquare) {
                        Assert.IsTrue(board.squares[squareAttackLeft + Directions.PawnForward] == Piece.WhitePawn, "Attempt to make an En Passant capture on a piece that isn't a White Pawn.");
                        moveList.Add(new Move(square, squareAttackLeft, Piece.WhitePawn,Piece.Empty, enPassantCapture: true));
                    } else if (squareAttackRight == board.enPassantSquare) {
                        Assert.IsTrue(board.squares[squareAttackRight + Directions.PawnForward] == Piece.WhitePawn, "Attempt to make an En Passant capture on a piece that isn't a White Pawn.");
                        moveList.Add(new Move(square, squareAttackRight, Piece.WhitePawn, Piece.Empty, enPassantCapture: true));
                    }
                }
            }
        }

        public static void PrintMoveList(List<Move> moveList) {
            StringBuilder sb = new StringBuilder();
            int numMoves = 1;
            sb.Append("Printing MoveList:");
            sb.AppendLine();
            foreach (Move move in moveList) {
                sb.Append("Move: " + numMoves + " > " + Move.GetMoveString(move));
                sb.AppendLine();
                numMoves++;
            }
            Debug.Log(sb.ToString());
        }
    }
}
