using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Bubble : MonoBehaviour
{
    public AudioClip popSound;  // assign in Inspector

    [Header("Bubble Data")]
    public int targetResult; // The number players need to create an equation for
    
    [Header("Movement")]
    public float fallSpeed = 3f;
    
    [Header("Visual Effects")]
    public float popAnimationDuration = 0.2f;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.3f;
    
    private TextMeshProUGUI displayText;

    public TextMeshProUGUI scoreText;
    private Button button;
    private RectTransform rectTransform;
    private float screenBottom;
    private Vector3 originalScale;
    private Vector2 originalPosition;
    
    void Start()
    {
        // Get UI components
        displayText = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        
        // Store original values
        originalScale = transform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        
        Debug.Log($"Bubble spawned at UI position: {originalPosition}");
        
        // Display only the target result
        if (displayText != null)
        {
            displayText.text = $"= {targetResult}";
        }
        
        if (button != null)
        {
            button.onClick.AddListener(OnBubbleClick);
        }
        
        // Calculate screen bottom for UI
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            screenBottom = -canvasRect.rect.height / 2 - 100;
        }
        else
        {
            screenBottom = -Screen.height / 2 - 50; // Fallback if no canvas found
            Debug.LogWarning("No Canvas found in parent hierarchy. Using fallback screen bottom calculation.");
        }
        
        Debug.Log($"Screen bottom calculated as: {screenBottom}");
    }
    
    void Update()
    {
        // Move bubble down
        if (rectTransform != null)
        {
            Vector2 currentPos = rectTransform.anchoredPosition;
            currentPos.y -= fallSpeed * Time.deltaTime;
            rectTransform.anchoredPosition = currentPos;
            
            // Check if bubble reached bottom
            if (currentPos.y < screenBottom)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoseLife();
                }
                Destroy(gameObject);
            }
        }
    }
    
    void OnBubbleClick()
    {
        // Check if player has made a valid equation that equals our target
        bool correctAnswer = GameManager.Instance.CheckAnswer(targetResult);
        
        if (correctAnswer)
        {
            StartCoroutine(PopAnimation());
        }
        else
        {   
            Debug.Log("Incorrect equation! Bubble will shake instead.");

            StartCoroutine(ShakeAnimation());
        }
    }
    
    IEnumerator PopAnimation()
    {
        float elapsedTime = 0;
        Vector3 startScale = originalScale;
        Vector3 endScale = Vector3.zero;
        
        while (elapsedTime < popAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / popAnimationDuration;
            
            // Ease out animation
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }
        AudioSource.PlayClipAtPoint(popSound, transform.position);
        Destroy(gameObject);
    }
    
    IEnumerator ShakeAnimation()
    {
        float elapsedTime = 0;
        
        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Random shake offset for UI
            Vector2 randomOffset = Random.insideUnitCircle * shakeIntensity * 10f;
            rectTransform.anchoredPosition = originalPosition + randomOffset;
            
            yield return null;
        }
        
        // Return to original position
        rectTransform.anchoredPosition = originalPosition;
    }
}