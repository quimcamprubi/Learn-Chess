using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public static class GameSettings {
        public static GameModeEnum GameMode;
        public static DifficultyEnum Difficulty;
        
        public enum GameModeEnum {
            ChallengeMode,
            LearningMode
        }

        public enum DifficultyEnum {
            Beginner,
            Amateur,
            Intermediate,
            Hard,
            Master
        }
    }
}

