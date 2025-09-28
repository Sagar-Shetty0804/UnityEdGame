using UnityEngine;

public class ColumnController : MonoBehaviour
{
    [Header("Column Settings")]
    public ColumnTile[] tiles; // Array of 4 tiles
    public float moveSpeed = 0f;

    private QuestionData assignedQuestion;
    private bool questionDisplayed = false;
    private bool questionAnswered = false;

    [Header("Question Display")]
    public float questionTriggerDistance = 10f; // Distance from player to show question
    public Transform player; // Assign your bird/player transform
    private bool hasScored = false;

    void Start()
    {
        // Get player reference if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Get a random question and assign it to this column
        AssignQuestion();
    }

    void Update()
    {
        // Move column to the left
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        // Check if we should display the question
        CheckQuestionDisplay();

        // Destroy column when it goes off screen
        if (transform.position.x < -15f)
        {
            DestroyColumn();
        }
    }

    private void AssignQuestion()
    {
        assignedQuestion = QuizManager.Instance.GetRandomQuestion();

        if (assignedQuestion != null && tiles != null && tiles.Length >= 4)
        {
            // Set up tiles with question options
            tiles[0].SetupQuizTile("A", assignedQuestion.optionA, this);
            tiles[1].SetupQuizTile("B", assignedQuestion.optionB, this);
            tiles[2].SetupQuizTile("C", assignedQuestion.optionC, this);
            tiles[3].SetupQuizTile("D", assignedQuestion.optionD, this);

            Debug.Log($"Question assigned: {assignedQuestion.question}");
        }
        else
        {
            Debug.LogWarning("No question available for this column");
        }
    }

    private void CheckQuestionDisplay()
    {
        if (player == null || questionDisplayed || assignedQuestion == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= questionTriggerDistance)
        {
            QuizManager.Instance.DisplayQuestion(assignedQuestion);
            questionDisplayed = true;
        }
    }

    public void OnTileHit(string option)
    {
        if (questionAnswered) return;

        questionAnswered = true;

        // Submit answer to quiz manager
        if (assignedQuestion != null)
        {
            QuizManager.Instance.SubmitAnswer(option);
        }
    }

    // Call this when bird passes through the column successfully
    public void OnColumnPassed()
    {
        if (!hasScored)
        {
            hasScored = true;
            // Additional scoring logic can go here if needed
        }
    }
    
    public void DestroyColumn()
    {
        if (questionDisplayed && !questionAnswered)
            {
                QuizManager.Instance.HideQuestion();
            }
            Destroy(gameObject);
    }
}