using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questionPanel;
    public TextMeshProUGUI questionText;

    public GameObject subjectPanel;
    public TextMeshProUGUI subjectText;
    public TextMeshProUGUI difficultyText;
    public GameObject explanationPanel;
    public TextMeshProUGUI explanationText;
    
    [Header("Settings")]
    public float questionDisplayDuration = 5f;
    public float explanationDisplayDuration = 3f;
    
    private QuestionDatabase questionDatabase;
    private QuestionData currentQuestion;
    private bool isQuestionActive = false;
    private List<QuestionData> usedQuestions = new List<QuestionData>();
    
    public static QuizManager Instance { get; private set; }
    
    // Events
    public System.Action<QuestionData> OnQuestionDisplayed;
    public System.Action<bool, string> OnAnswerSubmitted; // isCorrect, explanation
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadQuestions();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void LoadQuestions()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "questions.json");
        
        if (File.Exists(filePath))
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                questionDatabase = JsonUtility.FromJson<QuestionDatabase>(jsonContent);
                Debug.Log($"Loaded {questionDatabase.questions.Count} questions");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading questions: {e.Message}");
                CreateFallbackQuestions();
            }
        }
        else
        {
            Debug.LogWarning("Questions file not found, creating fallback questions");
            CreateFallbackQuestions();
        }
    }
    
    private void CreateFallbackQuestions()
    {
        questionDatabase = new QuestionDatabase();
        questionDatabase.questions = new List<QuestionData>
        {
            new QuestionData
            {
                id = 1,
                subject = "General",
                difficulty = 1,
                question = "What is 2 + 2?",
                optionA = "3",
                optionB = "4",
                optionC = "5",
                optionD = "6",
                correctAnswer = "B",
                explanation = "Basic addition: 2 + 2 = 4"
            }
        };
    }
    
    public QuestionData GetRandomQuestion()
    {
        if (questionDatabase == null || questionDatabase.questions.Count == 0)
        {
            Debug.LogError("No questions available!");
            return null;
        }
        
        // Reset used questions if we've used them all
        if (usedQuestions.Count >= questionDatabase.questions.Count)
        {
            usedQuestions.Clear();
        }
        
        // Get available questions
        List<QuestionData> availableQuestions = new List<QuestionData>();
        foreach (var question in questionDatabase.questions)
        {
            if (!usedQuestions.Contains(question))
            {
                availableQuestions.Add(question);
            }
        }
        
        if (availableQuestions.Count == 0)
        {
            return questionDatabase.questions[Random.Range(0, questionDatabase.questions.Count)];
        }
        
        QuestionData selectedQuestion = availableQuestions[Random.Range(0, availableQuestions.Count)];
        usedQuestions.Add(selectedQuestion);
        return selectedQuestion;
    }
    
    public void DisplayQuestion(QuestionData question)
    {
        if (question == null) return;
        
        currentQuestion = question;
        isQuestionActive = true;
        
        // Update UI
        questionText.text = question.question;
        subjectText.text = question.subject;
        difficultyText.text = GetDifficultyText(question.difficulty);
        
        // Show question panel
        questionPanel.SetActive(true);
        subjectPanel.SetActive(true);
        explanationPanel.SetActive(false);
        
        // Notify listeners
        OnQuestionDisplayed?.Invoke(question);
        
        Debug.Log($"Displaying question: {question.question}");
    }
    
    private string GetDifficultyText(int difficulty)
    {
        return difficulty switch
        {
            1 => "Easy",
            2 => "Medium",
            3 => "Hard",
            _ => "Normal"
        };
    }
    
    public void SubmitAnswer(string selectedOption)
    {
        if (!isQuestionActive || currentQuestion == null) return;
        
        bool isCorrect = currentQuestion.IsCorrectAnswer(selectedOption);
        string explanation = currentQuestion.explanation;
        
        // Hide question panel
        questionPanel.SetActive(false);
        subjectPanel.SetActive(false);
        
        // Show explanation
        ShowExplanation(isCorrect, explanation);
        
        // Notify listeners
        OnAnswerSubmitted?.Invoke(isCorrect, explanation);
        
        isQuestionActive = false;
        
        Debug.Log($"Answer submitted: {selectedOption}, Correct: {isCorrect}");
    }
    
    private void ShowExplanation(bool isCorrect, string explanation)
    {
        explanationPanel.SetActive(true);
        explanationText.text = $"{(isCorrect ? "✓ Correct!" : "✗ Wrong!")}\n{explanation}";
        
        StartCoroutine(HideExplanationAfterDelay());
    }
    
    private IEnumerator HideExplanationAfterDelay()
    {
        yield return new WaitForSeconds(explanationDisplayDuration);
        explanationPanel.SetActive(false);
    }
    
    public void HideQuestion()
    {
        questionPanel.SetActive(false);
        subjectPanel.SetActive(false);
        explanationPanel.SetActive(false);
        isQuestionActive = false;
    }
    
    public bool IsQuestionActive()
    {
        return isQuestionActive;
    }
    
    public QuestionData GetCurrentQuestion()
    {
        return currentQuestion;
    }
}