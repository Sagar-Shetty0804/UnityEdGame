using UnityEngine;
using TMPro;

public class ColumnTile : MonoBehaviour
{
    [Header("Quiz Integration")]
    public TextMeshProUGUI optionLabel; // Shows "A", "B", "C", "D" 
    public TextMeshProUGUI optionText;  // Shows the actual option text

    private SpriteRenderer spriteRenderer;
    private bool hasBeenPassed = false;

    // Quiz-related variables
    private string tileOption;
    private ColumnController parentColumn;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetupQuizTile(string option, string optionContent, ColumnController column)
    {
        tileOption = option;
        parentColumn = column;
        hasBeenPassed = false;

        // Update quiz option display
        if (optionLabel != null)
            optionLabel.text = option + ".";

        if (optionText != null)
            optionText.text = optionContent;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenPassed)
        {
            hasBeenPassed = true;

            // Notify parent column about the tile hit for quiz processing
            if (parentColumn != null && !string.IsNullOrEmpty(tileOption))
            {
                parentColumn.OnTileHit(tileOption);
            }

            // Check if this was the correct answer
            QuestionData currentQuestion = QuizManager.Instance.GetCurrentQuestion();
            bool isCorrectAnswer = false;

            if (currentQuestion != null)
            {
                isCorrectAnswer = currentQuestion.IsCorrectAnswer(tileOption);
            }

            if (isCorrectAnswer)
            {
                // Player chose correct answer - award points
                ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.AddScore(10);
                }
                Debug.Log($"Correct answer: {tileOption}! +10 points");

                // Notify column that it was passed successfully
                if (parentColumn != null)
                {
                    parentColumn.OnColumnPassed();
                }
            }
            else
            {
                // Player chose wrong answer - lose points and game over
                ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.AddScore(-5);
                }


                FlappyManager flappyManager = FindFirstObjectByType<FlappyManager>();
                if (flappyManager != null && scoreManager != null && scoreManager.GetCurrentScore() < 0)
                {
                    flappyManager.GameOver();
                }
                Debug.Log($"Wrong answer: {tileOption}! Game Over!");
                StartCoroutine(DestroyColumnAfterDelay(0.5f));
            }

            // Start visual feedback coroutine
            StartCoroutine(ShowAnswerFeedback(isCorrectAnswer));
        }
    }

    private System.Collections.IEnumerator ShowAnswerFeedback(bool isCorrect)
    {
        if (spriteRenderer == null) yield break;

        // Brief color feedback
        Color originalColor = spriteRenderer.color;
        Color feedbackColor = isCorrect ? Color.green : Color.red;

        // Flash effect
        spriteRenderer.color = feedbackColor;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = feedbackColor;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }
    
    // Add this coroutine at the end of the class:
    private System.Collections.IEnumerator DestroyColumnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ColumnController parentColumn = GetComponentInParent<ColumnController>();
        if (parentColumn != null)
        {
            parentColumn.DestroyColumn();
        }
    }
}