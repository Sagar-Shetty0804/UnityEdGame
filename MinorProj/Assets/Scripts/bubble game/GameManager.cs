using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int lives = 3;
    public float bubbleSpawnRate = 3f;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI scoreText;
    
    [Header("Current Selection Display")]
    public TextMeshProUGUI selectionText;
    
    [Header("Hearts System")]
    public Transform heartsContainer;
    public GameObject heartPrefab;
    public Image redFlashOverlay;
    public float flashDuration = 0.5f;
    public float flashIntensity = 0.3f;
    
    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    
    [Header("Game State")]
    public bool gameOver = false;
    
    private int score = 0;
    
    // Enhanced equation system for long equations
    private List<int> selectedNumbers = new List<int>();
    private List<string> selectedOperators = new List<string>();
    private bool isLongEquationMode = false;
    
    // Legacy system for backward compatibility
    private int firstNumber = 0;
    private int secondNumber = 0;
    private string selectedOperator = "";
    private bool hasFirstNumber = false;
    private bool hasSecondNumber = false;
    private bool hasSelectedOperator = false;
    
    private List<GameObject> heartObjects = new List<GameObject>();
    
    public static GameManager Instance;
    
    // Properties for other scripts to access - enhanced for long equations
    public bool HasValidSelection 
    { 
        get 
        {
            if (isLongEquationMode)
            {
                return selectedNumbers.Count >= 2 && selectedOperators.Count >= 1 && 
                       selectedNumbers.Count == selectedOperators.Count + 1;
            }
            else
            {
                return hasFirstNumber && hasSecondNumber && hasSelectedOperator;
            }
        }
    }
    
    public int FirstNumber => isLongEquationMode ? (selectedNumbers.Count > 0 ? selectedNumbers[0] : 0) : firstNumber;
    public int SecondNumber => isLongEquationMode ? (selectedNumbers.Count > 1 ? selectedNumbers[1] : 0) : secondNumber;
    public string SelectedOperator => isLongEquationMode ? (selectedOperators.Count > 0 ? selectedOperators[0] : "") : selectedOperator;
    
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
        UpdateSettingsBasedValues();
        
        // Make sure red flash starts invisible
        if (redFlashOverlay != null)
        {
            Color flashColor = redFlashOverlay.color;
            flashColor.a = 0;
            redFlashOverlay.color = flashColor;
            redFlashOverlay.gameObject.SetActive(true);
        }
        
        // Initialize Game Over Panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Setup restart button listener
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    void Update()
    {
        // Check for settings changes every frame
        UpdateSettingsBasedValues();
    }

    void UpdateSettingsBasedValues()
    {
        if (SettingsManager.Instance != null)
        {
            // Update long equation mode
            bool newLongEquationMode = SettingsManager.Instance.IsLongEquationEnabled();
            if (newLongEquationMode != isLongEquationMode)
            {
                isLongEquationMode = newLongEquationMode;
                ResetSelection();
                UpdateSelectionDisplay();
                Debug.Log($"Long equation mode: {isLongEquationMode}");
            }
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
        while (heartObjects.Count < 3)
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
        Color flashColor = Color.red;
        
        // Fade in red
        float elapsed = 0;
        while (elapsed < flashDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
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
        if (isLongEquationMode)
        {
            selectedNumbers.Add(number);
            Debug.Log($"Added number to long equation: {number} (Total numbers: {selectedNumbers.Count})");
        }
        else
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
                firstNumber = secondNumber;
                secondNumber = number;
                Debug.Log($"Replaced numbers - First: {firstNumber}, Second: {number}");
            }
        }
        
        UpdateSelectionDisplay();
        CheckForValidEquation();
    }
    
    public void SelectOperator(string op)
    {
        if (isLongEquationMode)
        {
            if (selectedNumbers.Count > selectedOperators.Count)
            {
                selectedOperators.Add(op);
                Debug.Log($"Added operator to long equation: {op} (Total operators: {selectedOperators.Count})");
            }
            else
            {
                Debug.Log("Cannot add operator: need a number first!");
            }
        }
        else
        {
            selectedOperator = op;
            hasSelectedOperator = true;
            Debug.Log($"Selected operator: {op}");
        }
        
        UpdateSelectionDisplay();
        CheckForValidEquation();
    }

    void UpdateSelectionDisplay()
    {
        if (selectionText != null)
        {
            string display = "";

            if (isLongEquationMode)
            {
                for (int i = 0; i < selectedNumbers.Count; i++)
                {
                    display += selectedNumbers[i].ToString();
                    
                    if (i < selectedOperators.Count)
                    {
                        display += " " + selectedOperators[i] + " ";
                    }
                    else if (i < selectedNumbers.Count - 1)
                    {
                        display += " _ ";
                    }
                }
                
                if (selectedNumbers.Count <= selectedOperators.Count)
                {
                    display += " _";
                }
                
                display += " = ?";
            }
            else
            {
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
            }

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
            if (isLongEquationMode)
            {
                int result = CalculateLongEquationResult();
                Debug.Log($"Complete long equation result: {result}");
            }
            else
            {
                int result = CalculateResult(firstNumber, selectedOperator, secondNumber);
                Debug.Log($"Complete equation: {firstNumber} {selectedOperator} {secondNumber} = {result}");
            }
        }
    }
    
    public bool CheckAnswer(int bubbleTargetResult)
    {
        if (!HasValidSelection)
        {
            Debug.Log("Please complete your equation first!");
            int penalty = isLongEquationMode ? selectedNumbers.Count + 1 : 1;
            score -= penalty;
            UpdateUI();
            return false;
        }
        
        int calculatedResult;
        int equationLength;
        
        if (isLongEquationMode)
        {
            calculatedResult = CalculateLongEquationResult();
            equationLength = selectedNumbers.Count + selectedOperators.Count;
            Debug.Log($"Checking long equation result: {calculatedResult} vs target {bubbleTargetResult}");
        }
        else
        {
            calculatedResult = CalculateResult(firstNumber, selectedOperator, secondNumber);
            equationLength = 3;
            Debug.Log($"Checking: {firstNumber} {selectedOperator} {secondNumber} = {calculatedResult} vs target {bubbleTargetResult}");
        }
        
        if (calculatedResult == bubbleTargetResult)
        {
            int points = isLongEquationMode ? (equationLength * 3) : 10;
            score += points;
            ResetSelection();
            UpdateUI();
            UpdateSelectionDisplay();
            Debug.Log($"Correct answer! +{points} points (Length bonus: {equationLength})");
            
            if (SettingsManager.Instance == null || SettingsManager.Instance.IsAutoResetTilesEnabled())
            {
                DynamicTileManager tileManager = FindFirstObjectByType<DynamicTileManager>();
                if (tileManager != null)
                {
                    tileManager.GenerateRandomTiles();
                }
            }
            
            return true;
        }
        else
        {   
            int penalty = isLongEquationMode ? (equationLength * 5) : 5;
            score -= penalty;
            ResetSelection();
            UpdateUI();
            UpdateSelectionDisplay();
            Debug.Log($"Wrong answer! -{penalty} points (Length penalty: {equationLength})");
            return false;
        }
    }
    
    int CalculateLongEquationResult()
    {
        if (selectedNumbers.Count == 0) return 0;
        
        int result = selectedNumbers[0];
        
        for (int i = 0; i < selectedOperators.Count && i + 1 < selectedNumbers.Count; i++)
        {
            string op = selectedOperators[i];
            int nextNumber = selectedNumbers[i + 1];
            
            switch (op)
            {
                case "+": result += nextNumber; break;
                case "-": result -= nextNumber; break;
                case "*": result *= nextNumber; break;
                case "/": result = nextNumber != 0 ? result / nextNumber : result; break;
                default: 
                    Debug.LogWarning($"Unknown operator: {op}");
                    break;
            }
        }
        
        return result;
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
        
        UpdateHearts();
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
        
        selectedNumbers.Clear();
        selectedOperators.Clear();
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
        
        // Stop time FIRST before showing panel
        Time.timeScale = 0;
        
        // Display game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over Panel activated");
        }
        else
        {
            Debug.LogWarning("Game Over Panel is not assigned!");
        }
        
        // Display final score
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score;
            Debug.Log($"Final score displayed: {score}");
        }
        else
        {
            Debug.LogWarning("Final Score Text is not assigned!");
        }
        
        // Log button status
        if (restartButton != null)
        {
            Debug.Log($"Restart button interactable: {restartButton.interactable}");
            Debug.Log($"Restart button active: {restartButton.gameObject.activeInHierarchy}");
        }
    }
    
    public void RestartGame()
    {
        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Reset game state
        Time.timeScale = 1;
        lives = 3;
        score = 0;
        gameOver = false;
        ResetSelection();
        InitializeHearts();
        UpdateUI();
        UpdateSelectionDisplay();
        
        Debug.Log("Game restarted!");
    }
    
    public int GetCurrentEquationLength()
    {
        if (isLongEquationMode)
        {
            return selectedNumbers.Count + selectedOperators.Count;
        }
        else
        {
            int length = 0;
            if (hasFirstNumber) length++;
            if (hasSelectedOperator) length++;
            if (hasSecondNumber) length++;
            return length;
        }
    }
    
    public bool IsLongEquationMode()
    {
        return isLongEquationMode;
    }
}