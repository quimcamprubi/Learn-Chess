using System;
using UnityEngine;

namespace Core {
    public class Move {
        public int move;
        public int score;
        
        public static int MaskEnPassant = 0x40000;
        public static int MaskPawnStart = 0x80000;
        public static int MaskCastlingMove = 0x1000000;
        public static int FromSquare(int move) { return move & 0x7F; }
        public static int ToSquare(int move) { return (move >> 7) & 0x7F; }
        public static int CapturedPiece(int move) { return (move >> 14) & 0xF; }
        public static int PromotedPiece(int move) { return (move >> 20) & 0xF; }
        public static bool IsEnPassantCapture(int move) { return (move & 0x40000) != 0; }
        public static bool IsPawnStartMove(int move) { return (move & 0x80000) != 0; }
        public static bool IsCastlingMove(int move) { return (move & 0x1000000) != 0; }
        public static bool IsMoveCapture(int move) { return (move & 0x7C000) != 0; }
        public static bool IsMovePromotion(int move) { return (move & 0xF00000) != 0; }
        public static int CreateMove(int fromSquare, int toSquare, int capturedPiece, int promotedPiece, bool enPassantCapture = false,
            bool pawnStart = false, bool castlingMove = false) {
            int move = fromSquare | (toSquare << 7) | (capturedPiece << 14) | (promotedPiece << 20);
            if (enPassantCapture) move |= MaskEnPassant;
            if (pawnStart) move |= MaskPawnStart;
            if (castlingMove) move |= MaskCastlingMove;
            return move;
        }
        public static void PrintDecMove(int move) { Debug.Log("Move: " + move); }
        public static void PrintHexMove(int move) { Debug.Log("Hex move: " + move.ToString("X")); }
        public static void PrintBinMove(int move) { Debug.Log("Binary move: " + Convert.ToString(move, 2)); }
        public static void PrintMoveData(int move) {
            Debug.Log("Move " + move.ToString("X") + "\nFrom: " + FromSquare(move) + "\nTo: " + ToSquare(move) + "\nCaptured: " 
                      + CapturedPiece(move) + "\nPromoted: " + PromotedPiece(move) + "\nEn Passant capture: " + IsEnPassantCapture(move)
                      + "\nPawn start: " + IsPawnStartMove(move) + "\nCastling move: " + IsCastlingMove(move) + "\n");
        }

    }
    
    /* ----- move variable -----
    Since the maximum index a move can be From or To is 98, we need 7 bits to represent the square indices.
    Pieces captured can go up to 13, meaning we need 4 bits to represent the captured piece. 
    Resulting masks:
    
    0000 0000 0000 0000 0000 0111 1111 -> From Square                       : 0x7F
    0000 0000 0000 0011 1111 1000 0000 -> To Square                         : >> 7, 0x7F   
    0000 0000 0011 1100 0000 0000 0000 -> Captured piece                    : >> 14, 0xF
    0000 0000 0100 0000 0000 0000 0000 -> En Passant capture (yes or no)    : 0x40000
    0000 0000 1000 0000 0000 0000 0000 -> Pawn Start (yes or no)            : 0x80000
    0000 1111 0000 0000 0000 0000 0000 -> Promoted piece                    : >> 20, 0xF
    0001 0000 0000 0000 0000 0000 0000 -> Castling move (yes or no)         : 0x1000000
    
    Is move a capture (any)?:
    0000 0000 0011 1100 0000 0000 0000 -> Captured piece
    0000 0000 0100 0000 0000 0000 0000 -> En Passant capture
    0000 0000 0111 1100 0000 0000 0000 -> Any sort of capture               : 0x7C000
    */
    
}
