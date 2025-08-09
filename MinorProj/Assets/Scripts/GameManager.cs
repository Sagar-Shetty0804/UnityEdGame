using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int lives = 3;
    public float bubbleSpawnRate = 3f;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI scoreText;
    
    [Header("Current Selection Display")]
    public TextMeshProUGUI selectionText; // Add this to show current equation
    
    [Header("Hearts System")]
    public Transform heartsContainer; // Drag your HeartsContainer here
    public GameObject heartPrefab; // Drag your heart prefab here
    public Image redFlashOverlay; // Drag your red overlay here
    public float flashDuration = 0.5f; // How long the red flash lasts
    public float flashIntensity = 0.3f; // How red the flash gets (0-1)
    
    [Header("Game State")]
    public bool gameOver = false;
    
    private int score = 0;
    private int firstNumber = 0;
    private int secondNumber = 0;
    private string selectedOperator = "";
    private bool hasFirstNumber = false;
    private bool hasSecondNumber = false;
    private bool hasSelectedOperator = false;
    
    private List<GameObject> heartObjects = new List<GameObject>(); // Keep track of heart objects
    
    public static GameManager Instance;
    
    // Properties for other scripts to access
    public bool HasValidSelection => hasFirstNumber && hasSecondNumber && hasSelectedOperator;
    public int FirstNumber => firstNumber;
    public int SecondNumber => secondNumber;
    public string SelectedOperator => selectedOperator;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeHearts();
        UpdateUI();
        UpdateSelectionDisplay();
        
        // Make sure red flash starts invisible
        if (redFlashOverlay != null)
        {
            Color flashColor = redFlashOverlay.color;
            flashColor.a = 0;
            redFlashOverlay.color = flashColor;
            redFlashOverlay.gameObject.SetActive(true); // Make sure it's active
        }
    }

    void InitializeHearts()
    {
        // First, collect all existing heart objects in the container
        heartObjects.Clear();
        
        if (heartsContainer != null)
        {
            // Get all child objects that are hearts
            for (int i = 0; i < heartsContainer.childCount; i++)
            {
                Transform child = heartsContainer.GetChild(i);
                if (child.name.Contains("Heart"))
                {
                    heartObjects.Add(child.gameObject);
                }
            }
        }
        
        // If we don't have enough hearts, create more
        while (heartObjects.Count < 3) // Always ensure we have at least 3 hearts
        {
            if (heartPrefab != null && heartsContainer != null)
            {
                GameObject newHeart = Instantiate(heartPrefab, heartsContainer);
                heartObjects.Add(newHeart);
            }
            else
            {
                Debug.LogWarning("Heart prefab or hearts container is missing!");
                break;
            }
        }
        
        // Update the display
        UpdateHearts();
    }
    
    void UpdateHearts()
    {
        // Show/hide hearts based on current lives
        for (int i = 0; i < heartObjects.Count; i++)
        {
            if (heartObjects[i] != null)
            {
                // Show heart if index is less than current lives
                bool shouldShow = i < lives;
                heartObjects[i].SetActive(shouldShow);
                
                Debug.Log($"Heart {i}: {(shouldShow ? "Active" : "Inactive")} (Lives: {lives})");
            }
        }
    }
    
    IEnumerator RedFlashEffect()
    {
        if (redFlashOverlay == null)
        {
            Debug.LogWarning("Red flash overlay is not assigned!");
            yield break;
        }
        
        Debug.Log("Starting red flash effect");
        
        Color originalColor = redFlashOverlay.color;
        Color flashColor = Color.red; // Use pure red
        
        // Fade in red
        float elapsed = 0;
        while (elapsed < flashDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            float alpha = Mathf.Lerp(0, flashIntensity, elapsed / (flashDuration / 2));
            flashColor.a = alpha;
            redFlashOverlay.color = flashColor;
            yield return null;
        }
        
        // Fade out red
        elapsed = 0;
        while (elapsed < flashDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(flashIntensity, 0, elapsed / (flashDuration / 2));
            flashColor.a = alpha;
            redFlashOverlay.color = flashColor;
            yield return null;
        }
        
        // Make sure it's completely transparent at the end
        flashColor.a = 0;
        redFlashOverlay.color = flashColor;
        
        Debug.Log("Red flash effect completed");
    }
    
    public void SelectNumber(int number)
    {
        if (!hasFirstNumber)
        {
            firstNumber = number;
            hasFirstNumber = true;
            Debug.Log($"Selected first number: {number}");
        }
        else if (!hasSecondNumber)
        {
            secondNumber = number;
            hasSecondNumber = true;
            Debug.Log($"Selected second number: {number}");
        }
        else
        {
            // If both numbers are selected, replace the first one and shift
            firstNumber = secondNumber;
            secondNumber = number;
            Debug.Log($"Replaced numbers - First: {firstNumber}, Second: {number}");
        }
        
        UpdateSelectionDisplay();
        CheckForValidEquation();
    }
    
    public void SelectOperator(string op)
    {
        selectedOperator = op;
        hasSelectedOperator = true;
        Debug.Log($"Selected operator: {op}");
        UpdateSelectionDisplay();
        CheckForValidEquation();
    }

    void UpdateSelectionDisplay()
    {
        if (selectionText != null)
        {
            string display = "";

            if (hasFirstNumber)
                display += firstNumber.ToString();
            else
                display += "_";

            display += " ";

            if (hasSelectedOperator)
                display += selectedOperator;
            else
                display += "_";

            display += " ";

            if (hasSecondNumber)
                display += secondNumber.ToString();
            else
                display += "_";

            display += " = ?";

            selectionText.text = display;
        }
        else
        {
            Debug.LogWarning("Selection Text is not assigned in the inspector!");
        }
    }
    
    void CheckForValidEquation()
    {
        if (HasValidSelection)
        {
            int result = CalculateResult(firstNumber, selectedOperator, secondNumber);
            Debug.Log($"Complete equation: {firstNumber} {selectedOperator} {secondNumber} = {result}");
        }
    }
    
    // Updated method - now only takes the target result from bubble
    public bool CheckAnswer(int bubbleTargetResult)
    {
        if (!HasValidSelection)
        {
            Debug.Log("Please select two numbers and an operator first!");
            score -= 1; // Penalize for invalid selection
            UpdateUI();
            return false;
            
        }
        
        int calculatedResult = CalculateResult(firstNumber, selectedOperator, secondNumber);
        
        Debug.Log($"Checking: {firstNumber} {selectedOperator} {secondNumber} = {calculatedResult} vs target {bubbleTargetResult}");
        
        if (calculatedResult == bubbleTargetResult)
        {
            score += 10;
            ResetSelection();
            UpdateUI();
            UpdateSelectionDisplay();
            Debug.Log("Correct answer! +10 points");
            
            // Trigger tile regeneration
            DynamicTileManager tileManager = FindFirstObjectByType<DynamicTileManager>();
            if (tileManager != null)
            {
                tileManager.GenerateRandomTiles();
            }
            
            return true;
        }
        else
        {   
            score -= 5;
            ResetSelection();
            UpdateUI();
            UpdateSelectionDisplay();
            Debug.Log("Wrong answer! -5 points");
            return false;
        }
    }
    
    int CalculateResult(int num1, string op, int num2)
    {
        switch (op)
        {
            case "+": return num1 + num2;
            case "-": return num1 - num2;
            case "*": return num1 * num2;
            case "/": return num2 != 0 ? num1 / num2 : 0;
            default: 
                Debug.LogWarning($"Unknown operator: {op}");
                return 0;
        }
    }
    
    public void LoseLife()
    {
        if (gameOver) return;
        
        lives--;
        Debug.Log($"Life lost! Lives remaining: {lives}");
        
        // Update hearts display
        UpdateHearts();
        
        // Trigger red flash effect
        StartCoroutine(RedFlashEffect());
        
        UpdateUI();
        
        if (lives <= 0)
        {
            GameOver();
        }
    }
    
    void ResetSelection()
    {
        hasFirstNumber = false;
        hasSecondNumber = false;
        hasSelectedOperator = false;
        firstNumber = 0;
        secondNumber = 0;
        selectedOperator = "";
    }
    
    public void ManualResetSelection()
    {
        ResetSelection();
        UpdateSelectionDisplay();
        Debug.Log("Selection manually reset!");
    }
    
    void UpdateUI()
    {
        if (livesText != null)
            livesText.text = "Lives: " + lives;
        
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
    
    void GameOver()
    {
        gameOver = true;
        Debug.Log($"Game Over! Final Score: {score}");
        
        // Stop time or disable game elements
        Time.timeScale = 0;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1;
        lives = 3;
        score = 0;
        gameOver = false;
        ResetSelection();
        InitializeHearts(); // Use InitializeHearts instead of CreateHearts
        UpdateUI();
        UpdateSelectionDisplay();
        Debug.Log("Game restarted!");
    }
}