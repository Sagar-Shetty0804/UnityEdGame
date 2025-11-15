using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("The unique name for this mountain range, e.g., 'Himalayas'")]
    public string mountainNameID;

    // NEW: Sound effect for an incorrect drop
    public AudioClip wrongDropSound;
    public AudioClip correctDropSound;

    // NEW: We'll use a single, central audio source to play sounds
    private static AudioSource audioSource;

    private Transform parentAfterDrag;
    private CanvasGroup canvasGroup;

    // NEW: Variables to remember where the item started
    private Transform originalParent;
    private Vector3 originalPosition;

    // NEW: A flag to check if the drop was on a valid zone
    private bool successfullyDropped = false;


    void Start()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // NEW: Find or create a central AudioSource for all items to use
        if (audioSource == null)
        {
            // Create a new GameObject just for playing sounds
            GameObject soundPlayer = new GameObject("Sound Player");
            audioSource = soundPlayer.AddComponent<AudioSource>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag: " + mountainNameID);

        // NEW: Remember the starting position and parent
        originalParent = transform.parent;
        originalPosition = transform.position;

        // NEW: Reset the drop flag at the start of a drag
        successfullyDropped = false;

        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root); // Move to top level to render above all UI
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");

        // NEW: Check if the item was dropped on a valid drop zone
        if (!successfullyDropped)
        {
            // If not, snap back to the original position
            transform.position = originalPosition;
            transform.SetParent(originalParent);

            // And play the "wrong" sound effect
            if (wrongDropSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(wrongDropSound);
            }
        }

        canvasGroup.blocksRaycasts = true;
    }

    // This method is called by the DropZone to set the new parent
    public void SetNewParent(Transform newParent)
    {
        parentAfterDrag = newParent;
        // NEW: Tell this item it was dropped successfully
        successfullyDropped = true;

        if (correctDropSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctDropSound);
        }
    }
    
    
}

// The new and improved script for your labels. It handles dragging,
// changing its appearance when unlocked, and being clickable.
/*using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Label Info")]
    public string mountainID; // e.g., "Himalayas"

    [Header("Visuals for Unlocking")]
    public Image backgroundImage; // The main image of the label
    public Color unlockedColor = Color.cyan; // A color to show it's unlocked

    [Header("Audio Feedback")]
    public AudioClip correctDropSound;
    public AudioClip wrongDropSound;
    private static AudioSource audioSource; // Shared audio source for all labels

    private Transform parentAfterDrag;
    [HideInInspector] public bool isUnlocked = false; // Hide from inspector but public for DropTarget
    private GameManager gameManager;
    private CanvasGroup canvasGroup;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Initialize a single, shared audio source for efficiency
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isUnlocked) return; // Cannot drag if already placed
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isUnlocked) return;
        // This method of dragging is smoother on scaled canvases
        GetComponent<RectTransform>().anchoredPosition += eventData.delta / transform.root.localScale.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isUnlocked) return;
        canvasGroup.blocksRaycasts = true;
        if (transform.parent == transform.root) { // If it wasn't dropped on a valid target
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
            
            // --- NEW: Play wrong sound on snap back ---
            if (wrongDropSound != null)
            {
                audioSource.PlayOneShot(wrongDropSound);
            }
        }
    }
    
    // This is called when the label is clicked with the mouse
    public void OnPointerClick(PointerEventData eventData)
    {
        if(isUnlocked)
        {
            // If it's unlocked, tell the GameManager to show its info
            gameManager.ShowInfoCard(mountainID);
        }
    }
    
    public void Unlock()
    {
        isUnlocked = true;
        if (backgroundImage != null)
        {
            backgroundImage.color = unlockedColor; // Change color to show it's unlocked
        }
        
        // --- NEW: Play correct sound on successful unlock ---
        if (correctDropSound != null)
        {
            audioSource.PlayOneShot(correctDropSound);
        }
    }
}*/


