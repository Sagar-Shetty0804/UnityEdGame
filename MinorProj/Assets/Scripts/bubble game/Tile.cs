using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    [Header("Tile Settings")]
    public bool isNumber = true;
    public int numberValue;
    public string operatorValue;
    
    [Header("Visual Feedback")]
    public Color numberSelectedColor = Color.green;
    public Color operatorSelectedColor = Color.blue;
    public Color numberDefaultColor = Color.white;
    public Color operatorDefaultColor = Color.cyan;
    public float feedbackDuration = 0.5f;
    
    private Button button;
    private Image image;
    private TextMeshProUGUI text;
    private Color originalColor;
    private bool isSelected = false;
    
    void Start()
    {
        SetupComponents();
        SetDefaultColors();
        UpdateDisplayText();
    }
    
    void SetupComponents()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        
        if (button == null)
        {
            Debug.LogError($"Button component missing on {gameObject.name}");
            return;
        }
        
        if (image == null)
        {
            Debug.LogError($"Image component missing on {gameObject.name}");
            return;
        }
        
        if (text == null)
        {
            Debug.LogError($"TextMeshPro component missing on {gameObject.name}");
            CreateTextComponent();
        }
        
        button.onClick.AddListener(OnTileClick);
    }
    
    void CreateTextComponent()
    {
        GameObject textChild = new GameObject("Text (TMP)");
        textChild.transform.SetParent(transform);
        text = textChild.AddComponent<TextMeshProUGUI>();
        
        text.text = "?";
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log($"Created TextMeshPro component for {gameObject.name}");
    }
    
    void SetDefaultColors()
    {
        if (image != null)
        {
            // Set different default colors for numbers vs operators
            Color defaultColor = isNumber ? numberDefaultColor : operatorDefaultColor;
            image.color = defaultColor;
            originalColor = defaultColor;
        }
    }
    
    void UpdateDisplayText()
    {
        if (text != null)
        {
            text.text = isNumber ? numberValue.ToString() : operatorValue;
            
            // Make operators slightly larger
            if (!isNumber)
            {
                text.fontSize = 28;
            }
            else
            {
                text.fontSize = 24;
            }
        }
    }
    
    void OnTileClick()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }
        
        if (isSelected) return;
        
        if (isNumber)
        {
            GameManager.Instance.SelectNumber(numberValue);
            ShowFeedback(numberSelectedColor);
            Debug.Log($"Player selected number: {numberValue}");
        }
        else
        {
            GameManager.Instance.SelectOperator(operatorValue);
            ShowFeedback(operatorSelectedColor);
            Debug.Log($"Player selected operator: {operatorValue}");
        }
    }
    
    void ShowFeedback(Color feedbackColor)
    {
        if (image == null) return;
        
        isSelected = true;
        image.color = feedbackColor;
        
        CancelInvoke("ResetColor");
        Invoke("ResetColor", feedbackDuration);
    }
    
    void ResetColor()
    {
        if (image != null)
        {
            image.color = originalColor;
        }
        isSelected = false;
    }
    
    public void ResetTile()
    {
        CancelInvoke("ResetColor");
        ResetColor();
        SetDefaultColors(); // Ensure correct default color is set
    }
    
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
            
            // Visual feedback for disabled state
            if (image != null)
            {
                Color currentColor = image.color;
                currentColor.a = interactable ? 1.0f : 0.5f;
                image.color = currentColor;
            }
        }
    }
    
    // Update tile content programmatically - CALLED BY DYNAMIC TILE MANAGER
    public void SetTileContent(bool isNumberTile, int number = 0, string operatorSymbol = "")
    {
        isNumber = isNumberTile;
        
        if (isNumber)
        {
            numberValue = number;
            operatorValue = "";
        }
        else
        {
            numberValue = 0;
            operatorValue = operatorSymbol;
        }
        
        SetDefaultColors();
        UpdateDisplayText();
        
        Debug.Log($"Tile {gameObject.name} updated: isNumber={isNumber}, value={numberValue}, operator='{operatorValue}', display='{text?.text}'");
    }
    
    // Force update display text - can be called anytime
    public void ForceUpdateDisplay()
    {
        UpdateDisplayText();
        SetDefaultColors();
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateDisplayText();
            SetDefaultColors();
        }
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnTileClick);
        }
        
        CancelInvoke();
    }
}