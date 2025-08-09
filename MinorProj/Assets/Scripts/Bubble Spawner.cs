using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject bubblePrefab;
    public float spawnRate = 4f;
    public float spawnRangeX = 400f;
    
    [Header("Canvas Reference")]
    public RectTransform bubbleCanvas;
    
    [Header("Difficulty")]
    public int maxNumber = 8;
    public int minNumber = 2; // Added minimum number for better gameplay
    
    private RectTransform spawnArea;
    
    void Start()
    {
        spawnArea = GetComponent<RectTransform>();
        
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
        
        StartCoroutine(SpawnBubbles());
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
            
            float randomDelay = Random.Range(spawnRate * 0.7f, spawnRate * 1.3f);
            yield return new WaitForSeconds(randomDelay);
            SpawnBubble();
        }
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
        
        // Add rotation
        bubble.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
        
        // NEW LOGIC: Just generate a target result number
        // The player will need to create an equation that equals this result
        int targetResult = Random.Range(minNumber, maxNumber * 2); // Allow for results from operations
        
        Bubble bubbleScript = bubble.GetComponent<Bubble>();
        if (bubbleScript != null)
        {
            bubbleScript.targetResult = targetResult; // Changed from 'result' to 'targetResult'
            bubbleScript.fallSpeed = Random.Range(15f, 30f);
        }
        
        Debug.Log($"Spawned bubble with target result: {targetResult}");
    }
}