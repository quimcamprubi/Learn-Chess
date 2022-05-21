using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void SetChallenge() {
        GameSettings.GameMode = GameSettings.GameModeEnum.ChallengeMode;
    }

    public void SetLearning() {
        GameSettings.GameMode = GameSettings.GameModeEnum.LearningMode;
    }

    public void SetBeginner() {
        GameSettings.Difficulty = GameSettings.DifficultyEnum.Beginner;
        SceneManager.LoadScene("GameScene");
    }
    
    public void SetAmateur() {
        GameSettings.Difficulty = GameSettings.DifficultyEnum.Amateur;
        SceneManager.LoadScene("GameScene");
    }
    
    public void SetIntermediate() {
        GameSettings.Difficulty = GameSettings.DifficultyEnum.Intermediate;
        SceneManager.LoadScene("GameScene");
    }
    
    public void SetHard() {
        GameSettings.Difficulty = GameSettings.DifficultyEnum.Hard;
        SceneManager.LoadScene("GameScene");
    }
    
    public void SetMaster() {
        GameSettings.Difficulty = GameSettings.DifficultyEnum.Master;
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
