using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject bubblePrefab;
    public float spawnRate = 3f;
    public float spawnRangeX = 400f;
    public int maxBubblesOnScreen = 3; // Limit concurrent bubbles
    
    [Header("Canvas Reference")]
    public RectTransform bubbleCanvas;
    
    [Header("Difficulty")]
    public int maxNumber = 6; // Maximum number for target result
    public int minNumber = 2; // Minimum number for better gameplay
    
    [Header("Difficulty-based Fall Speeds")]
    public float lowDifficultySpeed = 15f;      // Slow fall speed for Low difficulty
    public float mediumDifficultySpeed = 25f;   // Medium fall speed for Medium difficulty
    public float highDifficultySpeed = 40f;     // Fast fall speed for High difficulty
    public float speedVariation = 5f;           // Random variation in fall speed
    
    [Header("Smart Target Generation")]
    public bool useSmartTargets = true; // Generate targets based on available tiles
    
    private RectTransform spawnArea;
    private List<GameObject> activeBubbles = new List<GameObject>(); // Track active bubbles
    private DynamicTileManager tileManager;
    private int currentDifficulty = 1; // Cache difficulty to detect changes
    
    void Start()
    {
        spawnArea = GetComponent<RectTransform>();
        tileManager = FindFirstObjectByType<DynamicTileManager>();
        
        if (bubbleCanvas == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                bubbleCanvas = canvas.GetComponent<RectTransform>();
                Debug.Log("Auto-found canvas for bubble spawning");
            }
        }
        
        if (bubbleCanvas == null)
        {
            Debug.LogError("BubbleCanvas not assigned! Please assign it in the inspector.");
            return;
        }
        
        // Initialize difficulty
        UpdateDifficultySettings();
        
        StartCoroutine(SpawnBubbles());
    }
    
    void Update()
    {
        // Check for difficulty changes
        UpdateDifficultySettings();
    }
    
    void UpdateDifficultySettings()
    {
        if (SettingsManager.Instance != null)
        {
            int newDifficulty = SettingsManager.Instance.GetDifficultyLevel();
            if (newDifficulty != currentDifficulty)
            {
                currentDifficulty = newDifficulty;
                UpdateExistingBubbleSpeeds();
                Debug.Log($"Difficulty changed to: {GetDifficultyName(currentDifficulty)}");
            }
        }
    }
    
    void UpdateExistingBubbleSpeeds()
    {
        // Update the fall speed of existing bubbles when difficulty changes
        CleanupDestroyedBubbles();
        
        foreach (GameObject bubble in activeBubbles)
        {
            if (bubble != null)
            {
                Bubble bubbleScript = bubble.GetComponent<Bubble>();
                if (bubbleScript != null)
                {
                    bubbleScript.fallSpeed = GetFallSpeedForDifficulty();
                }
            }
        }
        
        Debug.Log($"Updated {activeBubbles.Count} existing bubbles with new fall speed");
    }
    
    float GetFallSpeedForDifficulty()
    {
        float baseSpeed;
        
        switch (currentDifficulty)
        {
            case 0: // Low
                baseSpeed = lowDifficultySpeed;
                break;
            case 1: // Medium
                baseSpeed = mediumDifficultySpeed;
                break;
            case 2: // High
                baseSpeed = highDifficultySpeed;
                break;
            default:
                baseSpeed = mediumDifficultySpeed;
                break;
        }
        
        // Add some random variation
        float variation = Random.Range(-speedVariation, speedVariation);
        return Mathf.Max(5f, baseSpeed + variation); // Minimum speed of 5
    }
    
    string GetDifficultyName(int level)
    {
        switch (level)
        {
            case 0: return "Low";
            case 1: return "Medium";
            case 2: return "High";
            default: return "Unknown";
        }
    }
    
    IEnumerator SpawnBubbles()
    {
        while (true)
        {
            // Don't spawn if game is over
            if (GameManager.Instance != null && GameManager.Instance.gameOver)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            
            // Clean up destroyed bubbles from our list
            CleanupDestroyedBubbles();
            
            // Only spawn if we haven't reached the maximum
            if (activeBubbles.Count < maxBubblesOnScreen)
            {
                SpawnBubble();
            }
            
            // Wait before checking again
            float randomDelay = Random.Range(spawnRate * 0.8f, spawnRate * 1.2f);
            yield return new WaitForSeconds(randomDelay);
        }
    }
    
    void CleanupDestroyedBubbles()
    {
        // Remove null references from our list
        activeBubbles.RemoveAll(bubble => bubble == null);
    }
    
    void SpawnBubble()
    {
        if (bubbleCanvas == null || bubblePrefab == null) return;
        
        // Get canvas dimensions
        Rect canvasRect = bubbleCanvas.rect;
        
        // Calculate spawn position
        float spawnY = canvasRect.height / 2 + Random.Range(10f, 50f);
        float maxX = canvasRect.width / 2 - 100;
        float spawnX = Random.Range(-maxX, maxX);
        
        Vector2 spawnPos = new Vector2(spawnX, spawnY);
        
        // Instantiate bubble
        GameObject bubble = Instantiate(bubblePrefab, bubbleCanvas);
        RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
        bubbleRect.anchoredPosition = spawnPos;
        
        // Add to our tracking list
        activeBubbles.Add(bubble);
        
        // Add rotation
        bubble.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
        
        // Generate target result
        int targetResult = GenerateTargetResult();
        
        // Set bubble properties based on current difficulty
        Bubble bubbleScript = bubble.GetComponent<Bubble>();
        if (bubbleScript != null)
        {
            bubbleScript.targetResult = targetResult;
            bubbleScript.fallSpeed = GetFallSpeedForDifficulty();
        }
        
        Debug.Log($"Spawned bubble with target result: {targetResult}, fall speed: {bubbleScript.fallSpeed:F1} (Difficulty: {GetDifficultyName(currentDifficulty)}, Active bubbles: {activeBubbles.Count})");
    }
    
    int GenerateTargetResult()
    {
        if (useSmartTargets && tileManager != null)
        {
            // Try to generate a target that can be created with current tiles
            return GenerateSmartTarget();
        }
        else
        {
            // Simple random generation with difficulty scaling
            int maxResult = maxNumber * (currentDifficulty + 1); // Scale with difficulty
            return Random.Range(minNumber, maxResult);
        }
    }
    
    int GenerateSmartTarget()
    {
        // Get available numbers and operators from tile manager
        List<int> availableNumbers = GetAvailableNumbers();
        List<string> availableOperators = GetAvailableOperators();
        
        if (availableNumbers.Count >= 2 && availableOperators.Count > 0)
        {
            // Try to create solvable equations 70% of the time
            if (Random.value < 0.7f)
            {
                int num1 = availableNumbers[Random.Range(0, availableNumbers.Count)];
                int num2 = availableNumbers[Random.Range(0, availableNumbers.Count)];
                string op = availableOperators[Random.Range(0, availableOperators.Count)];
                
                int result = CalculateResult(num1, op, num2);
                
                // Make sure result is reasonable based on difficulty
                int maxReasonableResult = maxNumber * (currentDifficulty + 2);
                if (result >= minNumber && result <= maxReasonableResult)
                {
                    return result;
                }
            }
        }
        
        // Fallback to random generation with difficulty scaling
        int maxResult = maxNumber * (currentDifficulty + 1);
        return Random.Range(minNumber, maxResult);
    }
    
    List<int> GetAvailableNumbers()
    {
        List<int> numbers = new List<int>();
        
        if (tileManager != null)
        {
            // Get numbers from tile manager if available
            // This assumes your DynamicTileManager has a way to get current numbers
            // You may need to add a public method to DynamicTileManager for this
            
            // For now, generate some reasonable numbers based on difficulty
            int maxNum = maxNumber + (currentDifficulty * 2);
            for (int i = 1; i <= maxNum; i++)
            {
                numbers.Add(i);
            }
        }
        
        return numbers;
    }
    
    List<string> GetAvailableOperators()
    {
        List<string> operators = new List<string> { "+", "-", "*" };
        
        // Add division for higher difficulties
        if (currentDifficulty >= 1)
        {
            operators.Add("/");
        }
        
        return operators;
    }
    
    int CalculateResult(int num1, string op, int num2)
    {
        switch (op)
        {
            case "+": return num1 + num2;
            case "-": return num1 - num2;
            case "*": return num1 * num2;
            case "/": return num2 != 0 ? num1 / num2 : num1;
            default: return num1 + num2;
        }
    }
    
    // Public method to manually spawn a bubble (useful for testing)
    public void ForceSpawnBubble()
    {
        if (activeBubbles.Count < maxBubblesOnScreen)
        {
            SpawnBubble();
        }
    }
    
    // Public method to get current bubble count
    public int GetActiveBubbleCount()
    {
        CleanupDestroyedBubbles();
        return activeBubbles.Count;
    }
    
    // Public method to clear all bubbles
    public void ClearAllBubbles()
    {
        foreach (GameObject bubble in activeBubbles)
        {
            if (bubble != null)
            {
                Destroy(bubble);
            }
        }
        activeBubbles.Clear();
    }
    
    // Public method to get current difficulty fall speed (for debugging/UI)
    public float GetCurrentFallSpeed()
    {
        return GetFallSpeedForDifficulty();
    }
}