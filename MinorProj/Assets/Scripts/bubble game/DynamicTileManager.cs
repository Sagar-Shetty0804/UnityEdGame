using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class DynamicTileManager : MonoBehaviour
{
    [Header("Tile Configuration")]
    public Tile[] allTiles;
    public int visibleTileCount = 15;
    
    [Header("Content Generation")]
    public int minNumber = 1;
    public int maxNumber = 11;
    public string[] availableOperators = {"+", "-", "*"};
    
    [Header("Reset Settings")]
    public Button resetButton;
    public float autoResetTime = 45f;
    
    [Header("Smart Tile Generation")]
    public bool generateHelpfulTiles = true;
    [Range(0f, 1f)]
    public float helpfulTileChance = 0.9f; // 90% chance to generate helpful tiles
    
    private List<Tile> activeTiles = new List<Tile>();
    private List<int> currentBubbleTargets = new List<int>();
    private float lastResetTime;
    private bool autoResetEnabled = true; // Cache the setting
    
    void Start()
    {
        if (allTiles == null || allTiles.Length == 0)
        {
            Debug.LogError("No tiles assigned to DynamicTileManager!");
            return;
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetTiles);
        }
        
        GenerateRandomTiles();
        lastResetTime = Time.time;
        
        // Initialize auto-reset setting
        UpdateAutoResetSetting();
    }
    
    void Update()
    {
        // Update auto-reset setting
        UpdateAutoResetSetting();
        
        // Only do auto-reset if it's enabled
        if (autoResetEnabled && Time.time - lastResetTime > autoResetTime)
        {
            ResetTiles();
        }
        
        if (generateHelpfulTiles)
        {
            UpdateBasedOnBubbles();
        }
    }
    
    void UpdateAutoResetSetting()
    {
        if (SettingsManager.Instance != null)
        {
            bool newAutoResetEnabled = SettingsManager.Instance.IsAutoResetTilesEnabled();
            if (newAutoResetEnabled != autoResetEnabled)
            {
                autoResetEnabled = newAutoResetEnabled;
                Debug.Log($"Auto-reset tiles setting changed to: {autoResetEnabled}");
                
                // Reset the timer when the setting changes
                if (autoResetEnabled)
                {
                    lastResetTime = Time.time;
                }
            }
        }
    }
    
    public void GenerateRandomTiles()
    {
        Debug.Log("Generating new random tiles...");
        
        HideAllTiles();
        activeTiles.Clear();
        
        List<Tile> tilesToActivate = SelectRandomTiles();
        
        // Get current bubble targets for smart generation
        UpdateCurrentBubbleTargets();
        
        // Generate a mix of helpful and random tiles
        GenerateMixedTiles(tilesToActivate);
        
        lastResetTime = Time.time;
        Debug.Log($"Generated {activeTiles.Count} active tiles");
    }
    
    void GenerateMixedTiles(List<Tile> tilesToActivate)
    {
        int helpfulTilesCount = 0;
        int maxHelpfulTiles = Mathf.RoundToInt(tilesToActivate.Count * helpfulTileChance);
        
        Debug.Log($"Generating content for {tilesToActivate.Count} tiles, {maxHelpfulTiles} will be helpful");
        
        for (int i = 0; i < tilesToActivate.Count; i++)
        {
            Tile tile = tilesToActivate[i];
            
            bool shouldGenerateHelpful = helpfulTilesCount < maxHelpfulTiles && 
                                       currentBubbleTargets.Count > 0 && 
                                       Random.Range(0f, 1f) < helpfulTileChance;
            
            Debug.Log($"Processing tile {i}: {tile.name}");
            
            if (shouldGenerateHelpful)
            {
                GenerateHelpfulTileContent(tile);
                helpfulTilesCount++;
            }
            else
            {
                GenerateRandomTileContent(tile);
            }
            
            activeTiles.Add(tile);
        }
        
        Debug.Log($"Successfully generated {activeTiles.Count} active tiles");
    }
    
    void GenerateHelpfulTileContent(Tile tile)
    {
        if (currentBubbleTargets.Count == 0)
        {
            GenerateRandomTileContent(tile);
            return;
        }
        
        // Pick a random bubble target to help with
        int targetResult = currentBubbleTargets[Random.Range(0, currentBubbleTargets.Count)];
        
        // Decide if this should be a number or operator
        bool shouldBeNumber = Random.Range(0f, 1f) < GetNumberProbability();
        
        if (shouldBeNumber)
        {
            GenerateHelpfulNumber(tile, targetResult);
        }
        else
        {
            GenerateRandomOperator(tile);
        }
        
        tile.gameObject.SetActive(true);
        tile.SetInteractable(true);
    }
    
    void GenerateHelpfulNumber(Tile tile, int targetResult)
    {
        List<int> helpfulNumbers = new List<int>();
        
        // For each operator, find numbers that could work
        foreach (string op in availableOperators)
        {
            for (int i = minNumber; i <= maxNumber; i++)
            {
                // Check if any combination with this number could equal our target
                if (CouldProduceTarget(i, op, targetResult) && 
                    i >= minNumber && i <= maxNumber)
                {
                    helpfulNumbers.Add(i);
                }
            }
        }
        
        int numberToUse;
        if (helpfulNumbers.Count > 0)
        {
            numberToUse = helpfulNumbers[Random.Range(0, helpfulNumbers.Count)];
            Debug.Log($"Generated helpful number: {numberToUse} for target {targetResult}");
        }
        else
        {
            numberToUse = Random.Range(minNumber, maxNumber + 1);
            Debug.Log($"Generated random number: {numberToUse} (no helpful options found)");
        }
        
        // USE SetTileContent instead of setting properties directly
        tile.SetTileContent(true, numberToUse, "");
    }
    
    bool CouldProduceTarget(int number, string op, int target)
    {
        // Check if this number with any other valid number could produce the target
        for (int other = minNumber; other <= maxNumber; other++)
        {
            if (CalculateResult(number, op, other, maxNumber) == target ||
                CalculateResult(other, op, number, maxNumber) == target)
            {
                return true;
            }
        }
        return false;
    }
    
    int CalculateResult(int num1, string op, int num2, int maxResult)
    {
        int result;
        switch (op)
        {
            case "+": result = num1 + num2; break;
            case "-": result = num1 - num2; break;
            case "*": result = num1 * num2; break;
            default: result = 0; break;
        }
        
        // Only return result if it's reasonable
        return (result >= 0 && result <= maxResult * 2) ? result : -999;
    }
    
    void GenerateRandomTileContent(Tile tile)
    {
        bool shouldBeNumber = Random.Range(0f, 1f) < GetNumberProbability();
        
        if (shouldBeNumber)
        {
            GenerateRandomNumber(tile);
        }
        else
        {
            GenerateRandomOperator(tile);
        }
        
        tile.gameObject.SetActive(true);
        tile.SetInteractable(true);
    }
    
    void GenerateRandomNumber(Tile tile)
    {
        int number = Random.Range(minNumber, maxNumber + 1);
        
        // USE SetTileContent instead of setting properties directly
        tile.SetTileContent(true, number, "");
        
        Debug.Log($"Generated random number tile: {number}");
    }
    
    void GenerateRandomOperator(Tile tile)
    {
        string operatorSymbol = availableOperators[Random.Range(0, availableOperators.Length)];
        
        // USE SetTileContent instead of setting properties directly
        tile.SetTileContent(false, 0, operatorSymbol);
        
        Debug.Log($"Generated operator tile: {operatorSymbol}");
    }
    
    float GetNumberProbability()
    {
        int numberCount = activeTiles.Count(t => t != null && t.isNumber);
        int operatorCount = activeTiles.Count - numberCount;
        
        // Maintain roughly 70% numbers, 30% operators
        if (operatorCount == 0) return 0.2f; // Force some operators
        if (numberCount == 0) return 0.9f;   // Force some numbers
        
        float numberRatio = (float)numberCount / activeTiles.Count;
        return numberRatio < 0.7f ? 0.8f : 0.2f;
    }
    
    void UpdateCurrentBubbleTargets()
    {
        currentBubbleTargets.Clear();
        
        // Find all active bubbles in the scene
        Bubble[] allBubbles = FindObjectsByType<Bubble>(FindObjectsSortMode.None);
        
        foreach (Bubble bubble in allBubbles)
        {
            if (bubble != null && bubble.gameObject.activeInHierarchy)
            {
                if (!currentBubbleTargets.Contains(bubble.targetResult))
                {
                    currentBubbleTargets.Add(bubble.targetResult);
                }
            }
        }
        
        Debug.Log($"Found {currentBubbleTargets.Count} bubble targets: {string.Join(", ", currentBubbleTargets)}");
    }
    
    void UpdateBasedOnBubbles()
    {
        List<int> newBubbleTargets = new List<int>();
        
        Bubble[] allBubbles = FindObjectsByType<Bubble>(FindObjectsSortMode.None);
        foreach (Bubble bubble in allBubbles)
        {
            if (bubble != null && bubble.gameObject.activeInHierarchy)
            {
                if (!newBubbleTargets.Contains(bubble.targetResult))
                {
                    newBubbleTargets.Add(bubble.targetResult);
                }
            }
        }
        
        // Check if bubble targets changed
        if (!ListsEqual(currentBubbleTargets, newBubbleTargets))
        {
            currentBubbleTargets = newBubbleTargets;
            Debug.Log($"Bubble targets changed: {string.Join(", ", currentBubbleTargets)}");
            
            // 40% chance to regenerate some tiles when bubbles change (only if auto-reset is enabled)
            if (autoResetEnabled && Random.Range(0f, 1f) < 0.4f)
            {
                RegenerateSomeTiles();
            }
        }
    }
    
    void RegenerateSomeTiles()
    {
        int tilesToRegenerate = Random.Range(2, 4);
        tilesToRegenerate = Mathf.Min(tilesToRegenerate, activeTiles.Count);
        
        List<Tile> tilesToUpdate = activeTiles.OrderBy(x => Random.value).Take(tilesToRegenerate).ToList();
        
        foreach (Tile tile in tilesToUpdate)
        {
            // 70% chance to generate helpful tile, 30% random
            if (Random.Range(0f, 1f) < 0.7f && currentBubbleTargets.Count > 0)
            {
                GenerateHelpfulTileContent(tile);
            }
            else
            {
                GenerateRandomTileContent(tile);
            }
        }
        
        Debug.Log($"Regenerated {tilesToRegenerate} tiles based on bubble changes");
    }
    
    List<Tile> SelectRandomTiles()
    {
        List<Tile> availableTiles = allTiles.Where(t => t != null).ToList();
        List<Tile> selectedTiles = new List<Tile>();
        
        int tilesToSelect = Mathf.Min(visibleTileCount, availableTiles.Count);
        
        Debug.Log($"Selecting {tilesToSelect} tiles from {availableTiles.Count} available tiles");
        
        for (int i = 0; i < tilesToSelect; i++)
        {
            if (availableTiles.Count == 0) break;
            
            int randomIndex = Random.Range(0, availableTiles.Count);
            Tile selectedTile = availableTiles[randomIndex];
            selectedTiles.Add(selectedTile);
            availableTiles.RemoveAt(randomIndex);
            
            Debug.Log($"Selected tile {i}: {selectedTile.name}");
        }
        
        return selectedTiles;
    }
    
    bool ListsEqual(List<int> list1, List<int> list2)
    {
        if (list1.Count != list2.Count) return false;
        
        var sorted1 = list1.OrderBy(x => x).ToList();
        var sorted2 = list2.OrderBy(x => x).ToList();
        
        for (int i = 0; i < sorted1.Count; i++)
        {
            if (sorted1[i] != sorted2[i]) return false;
        }
        return true;
    }
    
    void HideAllTiles()
    {
        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                tile.gameObject.SetActive(false);
                tile.ResetTile();
            }
        }
        Debug.Log($"Hidden all {allTiles.Length} tiles");
    }
    
    public void ResetTiles()
    {
        Debug.Log("Resetting tiles...");
        GenerateRandomTiles();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ManualResetSelection();
        }
    }
    
    // Public methods for external control
    public void SetVisibleTileCount(int count)
    {
        visibleTileCount = Mathf.Clamp(count, 1, allTiles.Length);
        ResetTiles();
    }
    
    public void SetDifficulty(int minNum, int maxNum)
    {
        minNumber = minNum;
        maxNumber = maxNum;
        
        // Store the current time scale
        float originalTimeScale = Time.timeScale;
        
        // Temporarily set time scale to 1 to allow operations to complete
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1f;
        }
        
        ResetTiles();
        
        // Restore the original time scale (don't force it to 1 if it was 0 for a reason)
        Time.timeScale = originalTimeScale;
    }
    
    public void SetHelpfulTileChance(float chance)
    {
        helpfulTileChance = Mathf.Clamp01(chance);
        ResetTiles();
    }
    
    public string GetActiveTilesInfo()
    {
        List<string> tileInfo = new List<string>();
        foreach (Tile tile in activeTiles)
        {
            if (tile != null)
            {
                if (tile.isNumber)
                    tileInfo.Add($"Number: {tile.numberValue}");
                else
                    tileInfo.Add($"Operator: {tile.operatorValue}");
            }
        }
        return string.Join(", ", tileInfo);
    }
    
    public List<int> GetCurrentBubbleTargets()
    {
        return new List<int>(currentBubbleTargets);
    }
    
    // Public method to check if auto-reset is currently enabled
    public bool IsAutoResetEnabled()
    {
        return autoResetEnabled;
    }
}