using System.Linq;

namespace Core {
    public static class Search {
        public static int PvTableSize = 130000;
        public static int MaxDepth = 64;
        public static void SearchPosition(Board board) {
            //TODO
        }

        public static bool IsRepeated(Board board) {
            for (int i = board.histPly - board.fiftyMoveCounter; i < board.histPly; i++) {
                if (board.gameHist[i].positionKey == board.positionKey) return true;
            }
            return false;
        }
    }
}
