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

        public static void AddCaptureMove(int fromSquare, int toSquare, int capture, List<Move> moveList, int attacker) {
            moveList.Add(new Move(fromSquare, toSquare, capture, Piece.Empty, score: Search.MvvLvaScores[capture, attacker] + 1000000));
        }

        public static void AddQuietMove(Move move, List<Move> moveList, Board board) {
            if (board.ply >= 0 && move.move == board.searchKillers[0, board.ply]) {
                move.score = 900000; // First killer. First Beta cutoff but worse than a capture.
            } else if (board.ply >= 0 && move.move == board.searchKillers[1, board.ply]) {
                move.score = 800000; // Second killer. Beta cutoff but worse than a capture.
            } else {
                move.score = board.searchHistory[board.squares[Move.FromSquare(move.move)], board.squares[Move.ToSquare(move.move)]];
            }
            if (Move.IsCastlingMove(move.move)) move.score++; 
            moveList.Add(move);
        }
        
        // White pieces
        public static void AddWhitePawnCaptureMove(int fromSquare, int toSquare, int capture, List<Move> moveList, int score) {
            Debug.Assert(Validations.IsPieceValidOrEmpty(capture), "Piece to capture is invalid.");
            Debug.Assert(Validations.IsSquareOnBoard(fromSquare), "From Square invalid in White Pawn capture move.");
            Debug.Assert(Validations.IsSquareOnBoard(toSquare), "To Square invalid in White Pawn capture move.");
            if (Board.squareRank[fromSquare] == (int) Constants.RanksEnum.RANK_7) { // Promotion
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.WhiteQueen, score: score));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.WhiteRook, score: score));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.WhiteBishop, score: score));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.WhiteKnight, score: score));
            } else {
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.Empty, score: score));
            }
        }
        public static void AddWhitePawnMove(int fromSquare, int toSquare, List<Move> moveList, Board board) {
            Debug.Assert(Validations.IsSquareOnBoard(fromSquare), "From Square invalid in White Pawn capture move.");
            Debug.Assert(Validations.IsSquareOnBoard(toSquare), "To Square invalid in White Pawn capture move.");
            if (Board.squareRank[fromSquare] == (int) Constants.RanksEnum.RANK_7) { // Promotion
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.WhiteQueen), moveList, board);
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.WhiteRook), moveList, board);
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.WhiteBishop), moveList, board);
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.WhiteKnight), moveList, board);
            } else {
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.Empty), moveList, board);
            }
        }
        
        // Black pieces
        public static void AddBlackPawnCaptureMove(int fromSquare, int toSquare, int capture, List<Move> moveList, int score) {
            Debug.Assert(Validations.IsPieceValidOrEmpty(capture), "Piece to capture is invalid.");
            Debug.Assert(Validations.IsSquareOnBoard(fromSquare), "From Square invalid in Black Pawn capture move.");
            Debug.Assert(Validations.IsSquareOnBoard(toSquare), "To Square invalid in Black Pawn capture move.");
            if (Board.squareRank[fromSquare] == (int) Constants.RanksEnum.RANK_2) { // Promotion
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.BlackQueen, score: score));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.BlackRook, score: score));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.BlackBishop, score: score));
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.BlackKnight, score: score));
            } else {
                moveList.Add(new Move(fromSquare, toSquare, capture, Piece.Empty, score: score));
            }
        }
        public static void AddBlackPawnMove(int fromSquare, int toSquare, List<Move> moveList, Board board) {
            Debug.Assert(Validations.IsSquareOnBoard(fromSquare), "From Square invalid in Black Pawn capture move.");
            Debug.Assert(Validations.IsSquareOnBoard(toSquare), "To Square invalid in Black Pawn capture move.");
            if (Board.squareRank[fromSquare] == (int) Constants.RanksEnum.RANK_2) { // Promotion
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.BlackQueen), moveList, board);
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.BlackRook), moveList, board);
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.BlackBishop), moveList, board);
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.BlackKnight), moveList, board);
            } else {
                AddQuietMove(new Move(fromSquare, toSquare, Piece.Empty, Piece.Empty), moveList, board);
            }
        }
        
        // Generate all moves in a given position
        public static List<Move> GenerateAllMoves(Board board) {
            List<Move> moveList = new List<Move>();
            Debug.Assert(board.CheckBoard());
            if (board.sideToPlay == Board.White) { // White pawns (and castling moves)
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[Piece.WhitePawn]; pieceNumber++) {
                    int square = board.pieceList[Piece.WhitePawn, pieceNumber];
                    Validations.IsSquareOnBoard(square);
                    if (board.squares[square + Directions.PawnForward] == Piece.Empty) { // Forward move and double forward move
                        AddWhitePawnMove(square, square + Directions.PawnForward, moveList, board);
                        if (Board.squareRank[square] == (int) Constants.RanksEnum.RANK_2 &&
                            board.squares[square + Directions.PawnDoubleForward] == Piece.Empty) {
                            AddQuietMove(new Move(square, square + Directions.PawnDoubleForward, 
                                Piece.Empty, Piece.Empty, pawnStart: true), moveList, board);
                        }
                    }
                    int squareAttackLeft = square + Directions.PawnCapLeft;
                    int squareAttackRight = square + Directions.PawnCapRight;
                    if (Validations.IsSquareOnBoard(squareAttackLeft) && Piece.PieceColor[board.squares[squareAttackLeft]] == Piece.Black) { // Capture left
                        int score = Search.MvvLvaScores[board.squares[squareAttackLeft], board.squares[square]];
                        AddWhitePawnCaptureMove(square, squareAttackLeft, board.squares[squareAttackLeft], moveList, score);
                    }
                    if (Validations.IsSquareOnBoard(squareAttackRight) && Piece.PieceColor[board.squares[squareAttackRight]] == Piece.Black) { // Capture right
                        int score = Search.MvvLvaScores[board.squares[squareAttackRight], board.squares[square]];
                        AddWhitePawnCaptureMove(square, squareAttackRight, board.squares[squareAttackRight], moveList, score);
                    }
                    if (squareAttackLeft == board.enPassantSquare) {
                        Debug.Assert(board.squares[squareAttackLeft - Directions.PawnForward] == Piece.BlackPawn, "Attempt to make an En Passant capture on a piece that isn't a Black Pawn.");
                        moveList.Add(new Move(square, squareAttackLeft, Piece.BlackPawn,Piece.Empty, true, score: 105 + 1000000));
                    }
                    if (squareAttackRight == board.enPassantSquare) {
                        Debug.Assert(board.squares[squareAttackRight - Directions.PawnForward] == Piece.BlackPawn, "Attempt to make an En Passant capture on a piece that isn't a Black Pawn.");
                        moveList.Add(new Move(square, squareAttackRight, Piece.BlackPawn, Piece.Empty, true, score: 105 + 1000000));
                    }
                }
                // Castling moves
                if ((board.castlingRights & (int) CastlingRightsEnum.WKCA) != 0) { // White Kingside castling
                    if (board.squares[(int) Board.Squares120Enum.F1] == Piece.Empty &&
                        board.squares[(int) Board.Squares120Enum.G1] == Piece.Empty) {
                        if (!board.IsSquareAttacked((int) Board.Squares120Enum.E1, Board.Black) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.F1, Board.Black) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.G1, Board.Black)) {
                            AddQuietMove(new Move((int) Board.Squares120Enum.E1, (int) Board.Squares120Enum.G1, Piece.Empty, Piece.Empty, castlingMove: true), moveList, board);
                            //moveList.Add(new Move((int) Board.Squares120Enum.E1, (int) Board.Squares120Enum.G1, Piece.Empty, Piece.Empty, castlingMove: true));
                        }
                    }
                } 
                if ((board.castlingRights & (int) CastlingRightsEnum.WQCA) != 0) { // White Queenside castling
                    if (board.squares[(int) Board.Squares120Enum.D1] == Piece.Empty &&
                        board.squares[(int) Board.Squares120Enum.C1] == Piece.Empty && 
                        board.squares[(int) Board.Squares120Enum.B1] == Piece.Empty) {
                        if (!board.IsSquareAttacked((int) Board.Squares120Enum.E1, Board.Black) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.D1, Board.Black) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.C1, Board.Black)) {
                            AddQuietMove(new Move((int) Board.Squares120Enum.E1, (int) Board.Squares120Enum.C1, Piece.Empty, Piece.Empty, castlingMove: true), moveList, board);
                            //moveList.Add(new Move((int) Board.Squares120Enum.E1, (int) Board.Squares120Enum.C1, Piece.Empty, Piece.Empty, castlingMove: true));
                        }
                    }
                } 
            }
            else { // Black pawns (and castling moves)
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[Piece.BlackPawn]; pieceNumber++) {
                    int square = board.pieceList[Piece.BlackPawn, pieceNumber];
                    Debug.Assert(Validations.IsSquareOnBoard(square));
                    if (board.squares[square - Directions.PawnForward] == Piece.Empty) { // Forward move and double forward move
                        AddBlackPawnMove(square, square - Directions.PawnForward, moveList, board);
                        if (Board.squareRank[square] == (int) Constants.RanksEnum.RANK_7 &&
                            board.squares[square - Directions.PawnDoubleForward] == Piece.Empty) {
                            AddQuietMove(new Move(square, square - Directions.PawnDoubleForward, 
                                Piece.Empty, Piece.Empty, pawnStart: true), moveList, board);
                        }
                    }
                    int squareAttackLeft = square - Directions.PawnCapLeft;
                    int squareAttackRight = square - Directions.PawnCapRight;
                    if (Validations.IsSquareOnBoard(squareAttackLeft) && Piece.PieceColor[board.squares[squareAttackLeft]] == Piece.White) { // Capture left
                        int score = Search.MvvLvaScores[board.squares[squareAttackLeft], board.squares[square]];
                        AddBlackPawnCaptureMove(square, squareAttackLeft, board.squares[squareAttackLeft], moveList, score);
                    }
                    if (Validations.IsSquareOnBoard(squareAttackRight) && Piece.PieceColor[board.squares[squareAttackRight]] == Piece.White) { // Capture right
                        int score = Search.MvvLvaScores[board.squares[squareAttackRight], board.squares[square]];
                        AddBlackPawnCaptureMove(square, squareAttackRight, board.squares[squareAttackRight], moveList, score);
                    }
                    if (squareAttackLeft == board.enPassantSquare) {
                        Debug.Assert(board.squares[squareAttackLeft + Directions.PawnForward] == Piece.WhitePawn, "Attempt to make an En Passant capture on a piece that isn't a White Pawn.");
                        moveList.Add(new Move(square, squareAttackLeft, Piece.WhitePawn,Piece.Empty, true, score: 105 + 1000000));
                    } else if (squareAttackRight == board.enPassantSquare) {
                        Debug.Assert(board.squares[squareAttackRight + Directions.PawnForward] == Piece.WhitePawn, "Attempt to make an En Passant capture on a piece that isn't a White Pawn.");
                        moveList.Add(new Move(square, squareAttackRight, Piece.WhitePawn, Piece.Empty, true, score: 105 + 1000000));
                    }
                }
                // Castling moves
                if ((board.castlingRights & (int) CastlingRightsEnum.BKCA) != 0) { // Black Kingside castling
                    if (board.squares[(int) Board.Squares120Enum.F8] == Piece.Empty &&
                        board.squares[(int) Board.Squares120Enum.G8] == Piece.Empty) {
                        if (!board.IsSquareAttacked((int) Board.Squares120Enum.E8, Board.White) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.F8, Board.White) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.G8, Board.White)) {
                            AddQuietMove(new Move((int) Board.Squares120Enum.E8, (int) Board.Squares120Enum.G8, Piece.Empty, Piece.Empty, castlingMove: true), moveList, board);
                            //moveList.Add(new Move((int) Board.Squares120Enum.E8, (int) Board.Squares120Enum.G8, Piece.Empty, Piece.Empty, castlingMove: true));
                        }
                    }
                } 
                if ((board.castlingRights & (int) CastlingRightsEnum.BQCA) != 0) { // Black Queenside castling
                    if (board.squares[(int) Board.Squares120Enum.D8] == Piece.Empty &&
                        board.squares[(int) Board.Squares120Enum.C8] == Piece.Empty && 
                        board.squares[(int) Board.Squares120Enum.B8] == Piece.Empty) {
                        if (!board.IsSquareAttacked((int) Board.Squares120Enum.E8, Board.White) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.D8, Board.White) &&
                            !board.IsSquareAttacked((int) Board.Squares120Enum.C8, Board.White)) {
                            AddQuietMove(new Move((int) Board.Squares120Enum.E8, (int) Board.Squares120Enum.C8, Piece.Empty, Piece.Empty, castlingMove: true), moveList, board);
                            //moveList.Add(new Move((int) Board.Squares120Enum.E8, (int) Board.Squares120Enum.C8, Piece.Empty, Piece.Empty, castlingMove: true));
                        }
                    }
                }
            }
            
            // Sliding pieces
            int side = board.sideToPlay;
            int pieceIndex = LoopSlideIndex[side];
            int piece = LoopSlidePieces[pieceIndex];
            while (piece != 0) {
                Debug.Assert(Validations.IsPieceValid(piece), "Invalid sliding piece");
                //Debug.Log("Sliders pieceIndex: " + pieceIndex + " - piece: " + piece);
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                    int square = board.pieceList[piece, pieceNumber];
                    Debug.Assert(Validations.IsSquareOnBoard(square), "Non-sliding piece offboard.");
                    int[] directionsList = Directions.PieceDirections[piece];
                    foreach (int direction in directionsList) {
                        int targetSquare = square + direction;
                        while (Validations.IsSquareOnBoard(targetSquare)) { // Keep sliding until we reach the end of the board
                            if (board.squares[targetSquare] != Piece.Empty) {
                                if (Piece.PieceColor[board.squares[targetSquare]] == (side ^ 1)) { // If piece belongs to opposing side, we capture
                                    AddCaptureMove(square, targetSquare, board.squares[targetSquare], moveList, board.squares[square]);
                                    //moveList.Add(new Move(square, targetSquare, board.squares[targetSquare], Piece.Empty));
                                }
                                break; // A piece of the same side is blocking the path, we can not slide any longer
                            }
                            AddQuietMove(new Move(square, targetSquare, Piece.Empty, Piece.Empty), moveList, board);
                            //moveList.Add(new Move(square, targetSquare, Piece.Empty, Piece.Empty));
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
                Debug.Assert(Validations.IsPieceValid(piece), "Invalid sliding piece");
                //Debug.Log("Non Sliders pieceIndex: " + pieceIndex + " - piece: " + piece);
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                    int square = board.pieceList[piece, pieceNumber];
                    Debug.Assert(Validations.IsSquareOnBoard(square), "Non-sliding piece offboard.");
                    int[] directionsList = Directions.PieceDirections[piece];
                    foreach (int direction in directionsList) {
                        int targetSquare = square + direction;
                        if (!Validations.IsSquareOnBoard(targetSquare)) continue; // If square is offboard, we ignore the move
                        if (board.squares[targetSquare] != Piece.Empty) {
                            if (Piece.PieceColor[board.squares[targetSquare]] == (side ^ 1)) { // If piece belongs to opposing side, we capture
                                AddCaptureMove(square, targetSquare, board.squares[targetSquare], moveList, board.squares[square]);
                                //moveList.Add(new Move(square, targetSquare, board.squares[targetSquare], Piece.Empty));
                            }
                            continue;
                        }
                        AddQuietMove(new Move(square, targetSquare, Piece.Empty, Piece.Empty), moveList, board);
                        //moveList.Add(new Move(square, targetSquare, Piece.Empty, Piece.Empty));
                    }
                }
                piece = LoopNonSlidePieces[++pieceIndex];
            }
            
            return moveList;
        }
        
        // Generate all capture moves in a given position
        public static List<Move> GenerateAllCaptures(Board board) {
            List<Move> moveList = new List<Move>();
            Debug.Assert(board.CheckBoard());
            if (board.sideToPlay == Board.White) { // White pawns
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[Piece.WhitePawn]; pieceNumber++) {
                    int square = board.pieceList[Piece.WhitePawn, pieceNumber];
                    Validations.IsSquareOnBoard(square);
                    int squareAttackLeft = square + Directions.PawnCapLeft;
                    int squareAttackRight = square + Directions.PawnCapRight;
                    if (Validations.IsSquareOnBoard(squareAttackLeft) && Piece.PieceColor[board.squares[squareAttackLeft]] == Piece.Black) { // Capture left
                        int score = Search.MvvLvaScores[board.squares[squareAttackLeft], board.squares[square]];
                        AddWhitePawnCaptureMove(square, squareAttackLeft, board.squares[squareAttackLeft], moveList, score);
                    }
                    if (Validations.IsSquareOnBoard(squareAttackRight) && Piece.PieceColor[board.squares[squareAttackRight]] == Piece.Black) { // Capture right
                        int score = Search.MvvLvaScores[board.squares[squareAttackRight], board.squares[square]];
                        AddWhitePawnCaptureMove(square, squareAttackRight, board.squares[squareAttackRight], moveList, score);
                    }
                    if (squareAttackLeft == board.enPassantSquare) {
                        Debug.Assert(board.squares[squareAttackLeft - Directions.PawnForward] == Piece.BlackPawn, "Attempt to make an En Passant capture on a piece that isn't a Black Pawn.");
                        moveList.Add(new Move(square, squareAttackLeft, Piece.BlackPawn,Piece.Empty, true, score: 105 + 1000000));
                    }
                    if (squareAttackRight == board.enPassantSquare) {
                        Debug.Assert(board.squares[squareAttackRight - Directions.PawnForward] == Piece.BlackPawn, "Attempt to make an En Passant capture on a piece that isn't a Black Pawn.");
                        moveList.Add(new Move(square, squareAttackRight, Piece.BlackPawn, Piece.Empty, true, score: 105 + 1000000));
                    }
                }
            }
            else { // Black pawns
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[Piece.BlackPawn]; pieceNumber++) {
                    int square = board.pieceList[Piece.BlackPawn, pieceNumber];
                    Debug.Assert(Validations.IsSquareOnBoard(square));
                    int squareAttackLeft = square - Directions.PawnCapLeft;
                    int squareAttackRight = square - Directions.PawnCapRight;
                    if (Validations.IsSquareOnBoard(squareAttackLeft) && Piece.PieceColor[board.squares[squareAttackLeft]] == Piece.White) { // Capture left
                        int score = Search.MvvLvaScores[board.squares[squareAttackLeft], board.squares[square]];
                        AddBlackPawnCaptureMove(square, squareAttackLeft, board.squares[squareAttackLeft], moveList, score);
                    }
                    if (Validations.IsSquareOnBoard(squareAttackRight) && Piece.PieceColor[board.squares[squareAttackRight]] == Piece.White) { // Capture right
                        int score = Search.MvvLvaScores[board.squares[squareAttackRight], board.squares[square]];
                        AddBlackPawnCaptureMove(square, squareAttackRight, board.squares[squareAttackRight], moveList, score);
                    }
                    if (squareAttackLeft == board.enPassantSquare) {
                        Debug.Assert(board.squares[squareAttackLeft + Directions.PawnForward] == Piece.WhitePawn, "Attempt to make an En Passant capture on a piece that isn't a White Pawn.");
                        moveList.Add(new Move(square, squareAttackLeft, Piece.WhitePawn,Piece.Empty, true, score: 105 + 1000000));
                    } else if (squareAttackRight == board.enPassantSquare) {
                        Debug.Assert(board.squares[squareAttackRight + Directions.PawnForward] == Piece.WhitePawn, "Attempt to make an En Passant capture on a piece that isn't a White Pawn.");
                        moveList.Add(new Move(square, squareAttackRight, Piece.WhitePawn, Piece.Empty, true, score: 105 + 1000000));
                    }
                }
            }
            
            // Sliding pieces
            int side = board.sideToPlay;
            int pieceIndex = LoopSlideIndex[side];
            int piece = LoopSlidePieces[pieceIndex];
            while (piece != 0) {
                Debug.Assert(Validations.IsPieceValid(piece), "Invalid sliding piece");
                //Debug.Log("Sliders pieceIndex: " + pieceIndex + " - piece: " + piece);
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                    int square = board.pieceList[piece, pieceNumber];
                    Debug.Assert(Validations.IsSquareOnBoard(square), "Non-sliding piece offboard.");
                    int[] directionsList = Directions.PieceDirections[piece];
                    foreach (int direction in directionsList) {
                        int targetSquare = square + direction;
                        while (Validations.IsSquareOnBoard(targetSquare)) { // Keep sliding until we reach the end of the board
                            if (board.squares[targetSquare] != Piece.Empty) {
                                if (Piece.PieceColor[board.squares[targetSquare]] == (side ^ 1)) { // If piece belongs to opposing side, we capture
                                    AddCaptureMove(square, targetSquare, board.squares[targetSquare], moveList, board.squares[square]);
                                }
                                break; // A piece of the same side is blocking the path, we can not slide any longer
                            }
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
                Debug.Assert(Validations.IsPieceValid(piece), "Invalid sliding piece");
                //Debug.Log("Non Sliders pieceIndex: " + pieceIndex + " - piece: " + piece);
                for (int pieceNumber = 0; pieceNumber < board.pieceNumbers[piece]; pieceNumber++) {
                    int square = board.pieceList[piece, pieceNumber];
                    Debug.Assert(Validations.IsSquareOnBoard(square), "Non-sliding piece offboard.");
                    int[] directionsList = Directions.PieceDirections[piece];
                    foreach (int direction in directionsList) {
                        int targetSquare = square + direction;
                        if (!Validations.IsSquareOnBoard(targetSquare)) continue; // If square is offboard, we ignore the move
                        if (board.squares[targetSquare] != Piece.Empty) {
                            if (Piece.PieceColor[board.squares[targetSquare]] == (side ^ 1)) { // If piece belongs to opposing side, we capture
                                AddCaptureMove(square, targetSquare, board.squares[targetSquare], moveList, board.squares[square]);
                                //moveList.Add(new Move(square, targetSquare, board.squares[targetSquare], Piece.Empty));
                            }
                        }
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

        public static bool MoveExists(Board board, Move move) { // Validates if there is any legal move that matches the parameter
            Move[] movesList = GenerateAllMoves(board).ToArray();
            foreach (Move move1 in movesList) {
                if (!board.MakeMove(move1)) continue;
                board.UnmakeMove();
                if (move1.move == move.move) return true;
            }
            return false;
        }
    }
}
