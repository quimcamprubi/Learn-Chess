using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace UI {
    public class PromotionMenu : MonoBehaviour {
        public static bool IsPromotionMenuOn = false;
        public GameObject promotionMenuUi;
        private Move moveToMake;
        private Game game;
        private int sideToPlay;

        public void PromoteToQueen() {
            Move finalMove = new Move(Move.FromSquare(moveToMake.move), Move.ToSquare(moveToMake.move),
                Move.CapturedPiece(moveToMake.move), sideToPlay == Piece.White ? Piece.WhiteQueen : Piece.BlackQueen);
            Game.GameIsPaused = false;
            promotionMenuUi.SetActive(false);
            Time.timeScale = 1f;
            game.MakePromotionMove(finalMove);
        }

        public void PromoteToRook() {
            Move finalMove = new Move(Move.FromSquare(moveToMake.move), Move.ToSquare(moveToMake.move),
                Move.CapturedPiece(moveToMake.move), sideToPlay == Piece.White ? Piece.WhiteRook : Piece.BlackRook);
            Game.GameIsPaused = false;
            promotionMenuUi.SetActive(false);
            Time.timeScale = 1f;
            game.MakePromotionMove(finalMove);
        }

        public void PromoteToBishop() {
            Move finalMove = new Move(Move.FromSquare(moveToMake.move), Move.ToSquare(moveToMake.move),
                Move.CapturedPiece(moveToMake.move), sideToPlay == Piece.White ? Piece.WhiteBishop : Piece.BlackBishop);
            Game.GameIsPaused = false;
            promotionMenuUi.SetActive(false);
            Time.timeScale = 1f;
            game.MakePromotionMove(finalMove);
        }

        public void PromoteToKnight() {
            Move finalMove = new Move(Move.FromSquare(moveToMake.move), Move.ToSquare(moveToMake.move),
                Move.CapturedPiece(moveToMake.move), sideToPlay == Piece.White ? Piece.WhiteKnight : Piece.BlackKnight);
            Game.GameIsPaused = false;
            promotionMenuUi.SetActive(false);
            Time.timeScale = 1f;
            game.MakePromotionMove(finalMove);
        }

        public void SelectPromotionMove(Move move, int side, Game gameClass) {
            Game.GameIsPaused = true;
            promotionMenuUi.SetActive(true);
            Time.timeScale = 0f;
            moveToMake = move;
            sideToPlay = side;
            game = gameClass;
        }
    }
}

