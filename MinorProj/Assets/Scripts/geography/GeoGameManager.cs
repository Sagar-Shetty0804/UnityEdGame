using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeoGameManager : MonoBehaviour
{
    [Header("Game Panels & Objects")]
    public GameObject instructionPanel;
    public GameObject mountainNamesContainer;
    public GameObject congratulationsPanel;
    public GameObject mapImage; // Reference to your map image
    public GameObject dropZonesContainer; // Reference to the parent of all drop zones

    [Header("Effects")]
    public ParticleSystem confettiEffect; 

    [Header("Timing Settings")]
    public float instructionDelay = 2f;
    public float instructionDuration = 3f;

    [Header("Game Elements")]
    public List<DropZone> allDropZones;

    private CanvasGroup mapCanvasGroup; // To control map transparency

    void Start()
    {
        // Get the CanvasGroup component from the map image for fading
        if (mapImage != null)
        {
            mapCanvasGroup = mapImage.GetComponent<CanvasGroup>();
            if (mapCanvasGroup == null)
            {
                // Add it if it doesn't exist, just in case
                mapCanvasGroup = mapImage.AddComponent<CanvasGroup>();
            }
        }
        
        // --- INITIAL SETUP ---
        // Ensure all game elements are hidden or in their starting state
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (congratulationsPanel != null) congratulationsPanel.SetActive(false);
        if (mountainNamesContainer != null) mountainNamesContainer.SetActive(false);
        if (dropZonesContainer != null) dropZonesContainer.SetActive(false); // Hide drop zones initially
        if (mapCanvasGroup != null) mapCanvasGroup.alpha = 1f; // Make sure map is fully visible

        // Start the game flow sequence
        StartCoroutine(GameFlow());
    }

    private IEnumerator GameFlow()
    {
        // 1. Wait for 2 seconds after the game starts
        yield return new WaitForSeconds(instructionDelay);

        // 2. Show instruction panel and blur (fade out) the map
        Debug.Log("Showing Instructions");
        if (instructionPanel != null) instructionPanel.SetActive(true);
        if (mapCanvasGroup != null) mapCanvasGroup.alpha = 0.5f; // Make map out of focus

        // 3. Wait for 3 seconds while instructions are visible
        yield return new WaitForSeconds(instructionDuration);

        // 4. Hide instruction panel and focus the map
        Debug.Log("Starting Gameplay");
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (mapCanvasGroup != null) mapCanvasGroup.alpha = 1f; // Bring map back into focus

        // 5. Show the draggable names and the drop zones
        if (mountainNamesContainer != null) mountainNamesContainer.SetActive(true);
        if (dropZonesContainer != null) dropZonesContainer.SetActive(true);
    }

    public void CheckForWinCondition()
    {
        foreach (DropZone zone in allDropZones)
        {
            if (!zone.IsCorrect())
            {
                return; 
            }
        }
        
        Debug.Log("GAME WON!");
        if (congratulationsPanel != null) congratulationsPanel.SetActive(true);
        if (mountainNamesContainer != null) mountainNamesContainer.SetActive(false);
    }
}

// The new brain of your game. It holds all the data, manages which panels are visible,
// and checks for the win condition.
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Data")]
    // You will fill this list in the Inspector with all your mountain info.
    public List<MountainData> allMountainData = new List<MountainData>();

    [Header("UI Panels")]
    public GameObject instructionPanel;
    public GameObject congratulationsPanel;
    public InfoCardController infoCard; // A direct link to our info card's script

    [Header("Game Flow Objects")]
    public GameObject labelsContainer;
    public GameObject dropZonesContainer;
    public CanvasGroup mapCanvasGroup; // To control map fade/blur

    [Header("Effects")] // --- NEW ---
    public ParticleSystem confettiEffect; // --- NEW ---

    private int unlockedMountainCount = 0;

    void Start()
    {
        // Initial setup - hide everything except the map
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (congratulationsPanel != null) congratulationsPanel.SetActive(false);
        if (infoCard != null) infoCard.gameObject.SetActive(false);
        if (labelsContainer != null) labelsContainer.SetActive(false);
        if (dropZonesContainer != null) dropZonesContainer.SetActive(false);

        // Start the initial game sequence
        StartCoroutine(GameFlow());
    }

    private IEnumerator GameFlow()
    {
        yield return new WaitForSeconds(2f); // Wait 2 seconds

        // Show instructions and fade out the map
        instructionPanel.SetActive(true);
        if (mapCanvasGroup != null) mapCanvasGroup.alpha = 0.5f;

        // In a real game, you'd wait for the close button. For now, we'll wait 3 seconds.
        yield return new WaitForSeconds(3f);

        // Hide instructions and fade the map back in
        instructionPanel.SetActive(false);
        if (mapCanvasGroup != null) mapCanvasGroup.alpha = 1f;

        // Show the gameplay elements
        labelsContainer.SetActive(true);
        dropZonesContainer.SetActive(true);
    }
    
    // Called from a DropTarget when a mountain is correctly placed
    public void MountainUnlocked(string mountainID)
    {
        unlockedMountainCount++;
        ShowInfoCard(mountainID);
        
        // Check if all mountains have been placed
        if (unlockedMountainCount >= allMountainData.Count)
        {
            Invoke("ShowCongratulations", 2f); // Show congrats after a 2-second delay
        }
    }

    // Finds the right mountain data and tells the info card to show it
    public void ShowInfoCard(string mountainID)
    {
        MountainData dataToShow = allMountainData.Find(mountain => mountain.mountainID == mountainID);
        if (dataToShow != null)
        {
            infoCard.gameObject.SetActive(true);
            infoCard.Populate(dataToShow);
        }
    }
    
    void ShowCongratulations()
    {
        // Hide gameplay elements and show the final panel
        labelsContainer.SetActive(false);
        dropZonesContainer.SetActive(false);
        congratulationsPanel.SetActive(true);

        // --- NEW ---
        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }
    }
}*/

