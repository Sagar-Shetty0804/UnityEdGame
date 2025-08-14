using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // Add this for new Input System

public class SettingsManager : MonoBehaviour
{
    [Header("Settings Overlay")]
    public GameObject settingsOverlay;
    public Button settingsButton;
    public Button closeButton;
    public Image backgroundDarkener;
    
    [Header("Setting Controls")]
    public Toggle longEquationToggle;
    public Toggle musicToggle;
    public TMP_Dropdown difficultyDropdown;
    public Toggle autoResetTilesToggle;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    // Settings values (public so other scripts can access them)
    [HideInInspector] public bool longEquationEnabled = false;
    [HideInInspector] public bool musicEnabled = true;
    [HideInInspector] public int difficultyLevel = 1; // 0=Low, 1=Medium, 2=High
    [HideInInspector] public bool autoResetTilesEnabled = true;
    
    // Singleton pattern
    public static SettingsManager Instance;
    
    private bool isSettingsOpen = false;
    private CanvasGroup overlayCanvasGroup;
    private Coroutine fadeCoroutine;
    
    // Input System variables
    private InputAction escapeAction;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Setup input action for escape key
        SetupInputActions();
        
        // Setup canvas group for smooth fading
        if (settingsOverlay != null)
        {
            overlayCanvasGroup = settingsOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = settingsOverlay.AddComponent<CanvasGroup>();
            }
        }
        
        // Setup button listeners
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);
        
        // Setup setting change listeners
        if (longEquationToggle != null)
            longEquationToggle.onValueChanged.AddListener(OnLongEquationChanged);
            
        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(OnMusicChanged);
            
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
            
        if (autoResetTilesToggle != null)
            autoResetTilesToggle.onValueChanged.AddListener(OnAutoResetTilesChanged);
        
        // Initialize UI with current settings
        InitializeUI();
        
        // Make sure overlay starts hidden
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }
    }
    
    void SetupInputActions()
    {
        // Create input action for escape key
        escapeAction = new InputAction("Escape", InputActionType.Button, "<Keyboard>/escape");
        escapeAction.performed += OnEscapePressed;
        escapeAction.Enable();
    }
    
    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (isSettingsOpen)
        {
            CloseSettings();
        }
    }
    
    void OnEnable()
    {
        if (escapeAction != null)
            escapeAction.Enable();
    }
    
    void OnDisable()
    {
        if (escapeAction != null)
            escapeAction.Disable();
    }
    
    void OnDestroy()
    {
        // Clean up input action
        if (escapeAction != null)
        {
            escapeAction.performed -= OnEscapePressed;
            escapeAction.Dispose();
        }
    }
    
    void InitializeUI()
    {
        // Load saved settings or use defaults
        LoadSettings();
        
        // Update UI to reflect current settings
        if (longEquationToggle != null)
            longEquationToggle.isOn = longEquationEnabled;
            
        if (musicToggle != null)
            musicToggle.isOn = musicEnabled;
            
        if (difficultyDropdown != null)
            difficultyDropdown.value = difficultyLevel;
            
        if (autoResetTilesToggle != null)
            autoResetTilesToggle.isOn = autoResetTilesEnabled;
    }
    
    public void OpenSettings()
    {
        if (isSettingsOpen) return;
        
        isSettingsOpen = true;
        
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
            
            // Start fade in animation
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        
        // Pause game when settings are open
        if (GameManager.Instance != null)
        {
            Time.timeScale = 0f;
        }
        
        Debug.Log("Settings opened");
    }
    
    public void CloseSettings()
    {
        if (!isSettingsOpen) return;
        
        // Start fade out animation
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut());
        
        // Save settings when closing
        SaveSettings();
        
        Debug.Log("Settings closed");
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.interactable = false;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime; // Use unscaled time since game is paused
                float progress = elapsedTime / fadeInDuration;
                float curveValue = fadeInCurve.Evaluate(progress);
                
                overlayCanvasGroup.alpha = curveValue;
                
                yield return null;
            }
            
            overlayCanvasGroup.alpha = 1f;
            overlayCanvasGroup.interactable = true;
        }
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 1f;
            overlayCanvasGroup.interactable = false;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / fadeOutDuration;
                float curveValue = fadeOutCurve.Evaluate(progress);
                
                overlayCanvasGroup.alpha = curveValue;
                
                yield return null;
            }
            
            overlayCanvasGroup.alpha = 0f;
        }
        
        // Hide overlay and resume game
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);
            
        if (GameManager.Instance != null)
            Time.timeScale = 1f;
            
        isSettingsOpen = false;
    }
    
    // Setting change handlers
    void OnLongEquationChanged(bool value)
    {
        longEquationEnabled = value;
        Debug.Log($"Long Equation setting changed to: {value}");
        // TODO: Implement long equation logic
    }
    
    void OnMusicChanged(bool value)
    {
        musicEnabled = value;
        Debug.Log($"Music setting changed to: {value}");
        // TODO: Implement music on/off logic
    }
    
    void OnDifficultyChanged(int value)
    {
        difficultyLevel = value;
        string difficultyName = GetDifficultyName(value);
        Debug.Log($"Difficulty setting changed to: {difficultyName} ({value})");
        // TODO: Implement difficulty logic
    }
    
    void OnAutoResetTilesChanged(bool value)
    {
        autoResetTilesEnabled = value;
        Debug.Log($"Auto Reset Tiles setting changed to: {value}");
        // TODO: Implement auto reset tiles logic
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
    
    // Save/Load settings (using PlayerPrefs for persistence)
    void SaveSettings()
    {
        PlayerPrefs.SetInt("LongEquation", longEquationEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Music", musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Difficulty", difficultyLevel);
        PlayerPrefs.SetInt("AutoResetTiles", autoResetTilesEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log("Settings saved");
    }
    
    void LoadSettings()
    {
        longEquationEnabled = PlayerPrefs.GetInt("LongEquation", 0) == 1;
        musicEnabled = PlayerPrefs.GetInt("Music", 1) == 1;
        difficultyLevel = PlayerPrefs.GetInt("Difficulty", 1);
        autoResetTilesEnabled = PlayerPrefs.GetInt("AutoResetTiles", 1) == 1;
        
        Debug.Log("Settings loaded");
    }
    
    // Public methods for other scripts to access settings
    public bool IsLongEquationEnabled() => longEquationEnabled;
    public bool IsMusicEnabled() => musicEnabled;
    public int GetDifficultyLevel() => difficultyLevel;
    public bool IsAutoResetTilesEnabled() => autoResetTilesEnabled;
    
    // Method to reset all settings to default
    public void ResetToDefaults()
    {
        longEquationEnabled = false;
        musicEnabled = true;
        difficultyLevel = 1;
        autoResetTilesEnabled = true;
        
        InitializeUI();
        SaveSettings();
        
        Debug.Log("Settings reset to defaults");
    }
}