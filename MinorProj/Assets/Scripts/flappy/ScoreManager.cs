using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;
    
    void Start()
    {
        UpdateScoreDisplay();
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
        Debug.Log($"Score added: {points}. Current score: {currentScore}");
    }
    
    public void HandleQuizAnswer(bool isCorrect)
{
        if (isCorrect)
        {
            AddScore(10);
        }
        else
        {
            AddScore(-5);
            // Trigger game over if needed
            // GameOver();
            FlappyManager flappyManager = FindFirstObjectByType<FlappyManager>();
            if (currentScore < 0) flappyManager.GameOver(); // Trigger game over if the score is negative
    }
}
    
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }
}