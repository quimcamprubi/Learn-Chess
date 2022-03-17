using Core;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu (menuName = "Theme/Piece Themes")]
    public class PieceThemes : ScriptableObject
    {
        public PieceColors whitePieces;
        public PieceColors blackPieces;

        [System.Serializable]
        public struct PieceColors
        {
            public Sprite Pawn;
            public Sprite Knight;
            public Sprite Bishop;
            public Sprite Rook;
            public Sprite Queen;
            public Sprite King;
        }

        public Sprite GetPieceSprite(int piece)
        {
            PieceColors colors = Piece.IsWhite(piece) ? whitePieces : blackPieces;
            switch (Piece.AbsolutePieceType(piece))
            {
                case Piece.Pawn:
                    return colors.Pawn;
                case Piece.Knight:
                    return colors.Knight;
                case Piece.Bishop:
                    return colors.Bishop;
                case Piece.Rook:
                    return colors.Rook;
                case Piece.Queen:
                    return colors.Queen;
                case Piece.King:
                    return colors.King;
                default:
                    Debug.Log(piece);
                    return null;
            }
        }
    }
}
