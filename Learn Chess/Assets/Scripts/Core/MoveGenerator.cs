using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Core {
    public static class MoveGenerator {
        public static readonly int[] LoopSlidePieces = {
            Piece.WhiteBishop, Piece.WhiteRook, Piece.WhiteQueen, 0, Piece.BlackBishop, Piece.BlackRook,
            Piece.BlackQueen, 0
        };
        public static readonly int[] LoopSlideIndex = { 0, 4 };
        public static readonly int[] LoopNonSlidePieces = { Piece.WhiteKnight, Piece.WhiteKing, 0, Piece.BlackKnight, Piece.BlackKing, 0 };
        public static readonly int[] LoopNonSlideIndex = { 0, 3 };
        
        public static List<Move> GenerateMoves(Board board) { 
            return new List<Move>();
        }

        // White pieces
        public static void AddWhitePawnCaptureMove(int fromSquare, int toSquare, int capture, List<Move> moveList) {
            Assert.IsTrue(Validations.IsPieceValidOrEmpty(capture), "Piece to capture is invalid.");
            Assert.IsTrue(Validations.IsSquareOnBoard(fromSquare), "From Square invalid in White Pawn capture move.");
            Assert.IsTrue(Validations.IsSquareOnBoard(toSquare), "To Square invalid in White Pawn capture move.");
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
            Assert.IsTrue(Validations.IsSquareOnBoard(fromSquare), "From Square invalid in White Pawn capture move.");
            Assert.IsTrue(Validations.IsSquareOnBoard(toSquare), "To Square invalid in White Pawn capture move.");
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
            Assert.IsTrue(Validations.IsPieceValidOrEmpty(capture), "Piece to capture is invalid.");
            Assert.IsTrue(Validations.IsSquareOnBoard(fromSquare), "From Square invalid in Black Pawn capture move.");
            Assert.IsTrue(Validations.IsSquareOnBoard(toSquare), "To Square invalid in Black Pawn capture move.");
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
            Assert.IsTrue(Validations.IsSquareOnBoard(fromSquare), "From Square invalid in Black Pawn capture move.");
            Assert.IsTrue(Validations.IsSquareOnBoard(toSquare), "To Square invalid in Black Pawn capture move.");
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
        public static List<Move> GenerateAllMoves(Board board) {
            List<Move> moveList = new List<Move>();
            board.CheckBoard();
            if (board.sideToPlay == Board.White) { // White pawns (and castling moves)
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
                // Castling moves
                if ((board.castlingRights & (int) CastlingRightsEnum.WKCA) != 0) { // White Kingside castling
                    if (board.squares[(int) Board.Squares120Enum.F1] == Piece.Empty &&
                        board.squares[(int) Board.Squares120Enum.G1] == Piece.Empty) {
                        if (!board.IsSquareAttacked((int) Board.Squares120Enum.F1, Board.Black) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.G1, Board.Black)) {
                            moveList.Add(new Move((int) Board.Squares120Enum.E1, (int) Board.Squares120Enum.G1, Piece.Empty, Piece.Empty, castlingMove: true));
                        }
                    }
                } 
                if ((board.castlingRights & (int) CastlingRightsEnum.WQCA) != 0) { // White Queenside castling
                    if (board.squares[(int) Board.Squares120Enum.D1] == Piece.Empty &&
                        board.squares[(int) Board.Squares120Enum.C1] == Piece.Empty && 
                        board.squares[(int) Board.Squares120Enum.B1] == Piece.Empty) {
                        if (!board.IsSquareAttacked((int) Board.Squares120Enum.D1, Board.Black) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.C1, Board.Black) && 
                            !board.IsSquareAttacked((int) Board.Squares120Enum.B1, Board.Black)) {
                            moveList.Add(new Move((int) Board.Squares120Enum.E1, (int) Board.Squares120Enum.C1, Piece.Empty, Piece.Empty, castlingMove: true));
                        }
                    }
                } 
            }
            else { // Black pawns (and castling moves)
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
                // Castling moves
                if ((board.castlingRights & (int) CastlingRightsEnum.BKCA) != 0) { // Black Kingside castling
                    if (board.squares[(int) Board.Squares120Enum.F8] == Piece.Empty &&
                        board.squares[(int) Board.Squares120Enum.G8] == Piece.Empty) {
                        if (!board.IsSquareAttacked((int) Board.Squares120Enum.F8, Board.White) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.G8, Board.White)) {
                            moveList.Add(new Move((int) Board.Squares120Enum.E8, (int) Board.Squares120Enum.G8, Piece.Empty, Piece.Empty, castlingMove: true));
                        }
                    }
                } 
                if ((board.castlingRights & (int) CastlingRightsEnum.BQCA) != 0) { // Black Queenside castling
                    if (board.squares[(int) Board.Squares120Enum.D8] == Piece.Empty &&
                        board.squares[(int) Board.Squares120Enum.C8] == Piece.Empty && 
                        board.squares[(int) Board.Squares120Enum.B8] == Piece.Empty) {
                        if (!board.IsSquareAttacked((int) Board.Squares120Enum.D8, Board.White) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.C8, Board.White) && 
                            !board.IsSquareAttacked((int) Board.Squares120Enum.B8, Board.White)) {
                            moveList.Add(new Move((int) Board.Squares120Enum.E8, (int) Board.Squares120Enum.G8, Piece.Empty, Piece.Empty, castlingMove: true));
                        }
                    }
                }
            }
            
            // Sliding pieces
            int side = board.sideToPlay;
            int pieceIndex = LoopSlideIndex[side];
            int piece = LoopSlidePieces[pieceIndex];
            while (piece != 0) {
                Assert.IsTrue(Validations.IsPieceValid(piece), "Invalid sliding piece");
                //Debug.Log("Sliders pieceIndex: " + pieceIndex + " - piece: " + piece);
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                    int square = board.pieceList[piece, pieceNumber];
                    Assert.IsTrue(Validations.IsSquareOnBoard(square), "Non-sliding piece offboard.");
                    List<int> directionsList = Directions.PieceDirections[piece];
                    foreach (int direction in directionsList) {
                        int targetSquare = square + direction;
                        while (Validations.IsSquareOnBoard(targetSquare)) { // Keep sliding until we reach the end of the board
                            if (board.squares[targetSquare] != Piece.Empty) {
                                if (Piece.PieceColor[board.squares[targetSquare]] == (side ^ 1)) { // If piece belongs to opposing side, we capture
                                    moveList.Add(new Move(square, targetSquare, board.squares[targetSquare], Piece.Empty));
                                }
                                break; // A piece of the same side is blocking the path, we can not slide any longer
                            }
                            moveList.Add(new Move(square, targetSquare, Piece.Empty, Piece.Empty));
                            targetSquare += direction;
                        }
                    }
                }
                piece = LoopSlidePieces[++pieceIndex];
            }
            
            // Non-sliding pieces
            pieceIndex = LoopNonSlideIndex[side];
            piece = LoopNonSlidePieces[pieceIndex];
            while (piece != 0) {
                Assert.IsTrue(Validations.IsPieceValid(piece), "Invalid sliding piece");
                //Debug.Log("Non Sliders pieceIndex: " + pieceIndex + " - piece: " + piece);
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                    int square = board.pieceList[piece, pieceNumber];
                    Assert.IsTrue(Validations.IsSquareOnBoard(square), "Non-sliding piece offboard.");
                    List<int> directionsList = Directions.PieceDirections[piece];
                    foreach (int direction in directionsList) {
                        int targetSquare = square + direction;
                        if (!Validations.IsSquareOnBoard(targetSquare)) continue; // If square is offboard, we ignore the move
                        if (board.squares[targetSquare] != Piece.Empty) {
                            if (Piece.PieceColor[board.squares[targetSquare]] == (side ^ 1)) { // If piece belongs to opposing side, we capture
                                moveList.Add(new Move(square, targetSquare, board.squares[targetSquare], Piece.Empty));
                            }
                            continue;
                        }
                        moveList.Add(new Move(square, targetSquare, Piece.Empty, Piece.Empty));
                    }
                }
                piece = LoopNonSlidePieces[++pieceIndex];
            }
            
            return moveList;
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
