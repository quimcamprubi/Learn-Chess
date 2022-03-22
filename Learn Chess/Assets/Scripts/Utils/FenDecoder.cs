using System;
using System.Collections.Generic;
using Core;

namespace Utils
{
    public static class FenDecoder
    {
        private static int WKCA = 1;
        private static int WQCA = 2;
        private static int BKCA = 4;
        private static int BQCA = 8;
        public static Dictionary<char, int> fenToPieceDictionary = new Dictionary<char, int>()
        {
            ['p'] = Piece.BlackPawn, ['n'] = Piece.BlackKnight, ['b'] = Piece.BlackBishop, ['r'] = Piece.BlackRook, ['q'] = Piece.BlackQueen,
            ['k'] = Piece.BlackKing, ['P'] = Piece.WhitePawn, ['N'] = Piece.WhiteKnight, ['B'] = Piece.WhiteBishop, ['R'] = Piece.WhiteRook, ['Q'] = Piece.WhiteQueen,
            ['K'] = Piece.WhiteKing
        };
        public static ChessPosition DecodePositionFromFen(string fen)
        {
            // Piece positions
            ChessPosition positionToReturn = new ChessPosition();
            string[] sections = fen.Split(' ');
            int file = 0;
            int rank = 7;
            foreach (char symbol in sections[0])
            {
                if (symbol == '/')
                {
                    file = 0;
                    rank--; // next rank
                }
                else
                {
                    if (char.IsDigit(symbol)) // Skip empty squares
                    {
                        file += (int) Char.GetNumericValue(symbol);
                    }
                    else
                    {
                        int type = fenToPieceDictionary[symbol];
                        positionToReturn.squares[Board.sq120(rank * 8 + file)] = type;
                        file++;
                    }
                }
            }
            positionToReturn.whiteToMove = (sections[1] == "w");

            // Castling Rights
            string castlingRights = sections[2];
            if (castlingRights.Contains("K")) positionToReturn.castlingRights |= WKCA;
            if (castlingRights.Contains("Q")) positionToReturn.castlingRights |= WQCA;
            if (castlingRights.Contains("k")) positionToReturn.castlingRights |= BKCA;
            if (castlingRights.Contains("q")) positionToReturn.castlingRights |= BQCA;

            // En Passant
            if (sections.Length > 3) {
                if (sections[3] != "-")
                {
                    int enPassantFile = sections[3][0] - 'a';
                    int enPassantRank = sections[3][1] - '1';
                    positionToReturn.enPassantSquare = Board.frTo120Sq(enPassantFile, enPassantRank);
                }
                
            }

            // Half-move clock
            if (sections.Length > 4) {
                int.TryParse (sections[4], out positionToReturn.plyCount);
            }
            
            return positionToReturn;
        }
    }
}