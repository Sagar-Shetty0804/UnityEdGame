using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlappyManager : MonoBehaviour
{
    [Header("Game References")]
    public BirdController birdController;
    public TextMeshProUGUI scoreText; // Only read from this, don't modify
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    
    [Header("Column System")]
    public ColumnSpawner columnSpawner;
    public ScoreManager scoreManager;
    
    [Header("Boundary Settings")]
    public float topBoundary = 6f;
    public float bottomBoundary = -6f;
    public float sideBoundary = 12f;
    
    [Header("Game Settings")]
    public bool useScreenBounds = true; // Automatically calculate bounds based on camera
    
    private bool isGameActive = true;
    private Camera mainCamera;
    
    void Start()
    {
        // Get the main camera for screen bounds calculation
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<Camera>();
            
        // Calculate screen boundaries if enabled
        if (useScreenBounds)
            CalculateScreenBounds();
            
        // Setup restart button
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        // Initialize game
        InitializeGame();
        
        Debug.Log("Flappy Manager Started!");
        Debug.Log($"Boundaries - Top: {topBoundary}, Bottom: {bottomBoundary}, Side: {sideBoundary}");
    }
    
    void Update()
    {
        if (isGameActive && birdController != null)
        {
            CheckBirdBounds();
        }
    }
    
    void CalculateScreenBounds()
    {
        if (mainCamera != null)
        {
            // Convert screen bounds to world coordinates
            Vector3 topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.nearClipPlane));
            Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            
            topBoundary = topRight.y + 1f; // Add some padding
            bottomBoundary = bottomLeft.y - 1f;
            sideBoundary = topRight.x + 2f; // Extra padding for side bounds
            
            Debug.Log($"Screen bounds calculated - Width: {Screen.width}, Height: {Screen.height}");
            Debug.Log($"World bounds - Top: {topBoundary}, Bottom: {bottomBoundary}, Side: {sideBoundary}");
        }
    }
    
    void CheckBirdBounds()
    {
        Vector3 birdPosition = birdController.transform.position;
        
        // Check if bird is out of bounds
        bool outOfBounds = false;
        string boundaryHit = "";
        
        if (birdPosition.y > topBoundary)
        {
            outOfBounds = true;
            boundaryHit = "top";
        }
        else if (birdPosition.y < bottomBoundary)
        {
            outOfBounds = true;
            boundaryHit = "bottom";
        }
        else if (Mathf.Abs(birdPosition.x) > sideBoundary)
        {
            outOfBounds = true;
            boundaryHit = "side";
        }
        
        if (outOfBounds)
        {
            Debug.Log($"Bird went out of bounds: {boundaryHit} boundary hit!");
            GameOver();
        }
    }
    
    public void GameOver()
    {
        if (!isGameActive) return; // Prevent multiple calls
        
        isGameActive = false;
        Debug.Log("Game Over!");
        
        // Stop the bird
        if (birdController != null)
            birdController.StopGame();
            
        // Stop column spawning
        if (columnSpawner != null)
            columnSpawner.StopSpawning();
            
        // Get final score from score manager instead of UI text
        if (finalScoreText != null && scoreManager != null)
        {
            finalScoreText.text = "Final Score: " + scoreManager.GetCurrentScore();
        }
        else if (finalScoreText != null && scoreText != null)
        {
            // Fallback to reading from UI text
            string currentScoreText = scoreText.text;
            string scoreValue = ExtractScoreFromText(currentScoreText);
            finalScoreText.text = "Final Score: " + scoreValue;
        }
            
        // Show game over UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    
    private string ExtractScoreFromText(string scoreText)
    {
        // Extract numeric value from score text (handles formats like "Score: 10", "Score : 10", etc.)
        if (string.IsNullOrEmpty(scoreText))
            return "0";
            
        // Find the last number in the string
        string[] parts = scoreText.Split(' ', ':');
        for (int i = parts.Length - 1; i >= 0; i--)
        {
            if (int.TryParse(parts[i].Trim(), out int score))
            {
                return score.ToString();
            }
        }
        
        return "0"; // Default if no number found
    }
    
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        
        // Reset game state
        isGameActive = true;
        
        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Reset bird
        if (birdController != null)
            birdController.ResetBird();
            
        // Reset score
        if (scoreManager != null)
            scoreManager.ResetScore();
            
        // Destroy existing columns
        GameObject[] existingColumns = GameObject.FindGameObjectsWithTag("Column");
        foreach (GameObject column in existingColumns)
        {
            Destroy(column);
        }
        
        // Restart column spawning
        if (columnSpawner != null)
            columnSpawner.StartSpawning();
            
        Debug.Log("Game restarted!");
    }
    
    void InitializeGame()
    {
        isGameActive = true;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Reset score at start
        if (scoreManager != null)
            scoreManager.ResetScore();
    }
    
    // Public getter for game state
    public bool IsGameActive()
    {
        return isGameActive;
    }
    
    // Method to set custom boundaries if needed
    public void SetBoundaries(float top, float bottom, float side)
    {
        topBoundary = top;
        bottomBoundary = bottom;
        sideBoundary = side;
        Debug.Log($"Custom boundaries set - Top: {top}, Bottom: {bottom}, Side: {side}");
    }
    
    // Gizmos for visualizing boundaries in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        // Draw top boundary
        Gizmos.DrawLine(new Vector3(-sideBoundary, topBoundary, 0), new Vector3(sideBoundary, topBoundary, 0));
        
        // Draw bottom boundary
        Gizmos.DrawLine(new Vector3(-sideBoundary, bottomBoundary, 0), new Vector3(sideBoundary, bottomBoundary, 0));
        
        // Draw side boundaries
        Gizmos.DrawLine(new Vector3(-sideBoundary, bottomBoundary, 0), new Vector3(-sideBoundary, topBoundary, 0));
        Gizmos.DrawLine(new Vector3(sideBoundary, bottomBoundary, 0), new Vector3(sideBoundary, topBoundary, 0));
        
        // Draw boundary labels (visible in Scene view)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(sideBoundary * 2, topBoundary - bottomBoundary, 0));
    }
}