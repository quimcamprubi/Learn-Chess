namespace Core {
    public static class Directions {
        public static int[] KnightDirections = new int[8] {-8, -19, -21, -12, 8, 19, 21, 12};
        public static int[] BishopDirections = new int[4] {-9, -11, 11, 9};
        public static int[] RookDirections = new int[4] {-1, -10, 1, 10};
        public static int[] KingDirections = new int[8] {-1, -10, 1, 10, -9, -11, 11, 9};
        public static int PawnForward = 10;             // Simple forward move
        public static int PawnDoubleForward = 20;       // Double forward move (Pawn Start)
        public static int PawnCapLeft = 9;              // Capture left
        public static int PawnCapRight = 11;            // Capture right
        
    }
}
