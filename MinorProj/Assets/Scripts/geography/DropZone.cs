using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Tooltip("The correct mountain name that should be dropped here, e.g., 'Himalayas'")]
    public string correctMountainNameID;
    
    private DraggableItem currentItem = null;
    private GeoGameManager gameManager; // NEW: A reference to the GameManager

    void Start()
    {
        // NEW: Find the GameManager in the scene when the game starts.
        gameManager = FindObjectOfType<GeoGameManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        DraggableItem draggableItem = droppedObject.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            if (draggableItem.mountainNameID == correctMountainNameID)
            {
                draggableItem.transform.SetParent(transform);
                draggableItem.transform.position = transform.position;
                draggableItem.SetNewParent(transform); 
                currentItem = draggableItem;

                // NEW: If the drop was correct, tell the GameManager to check if the game is won.
                if (gameManager != null)
                {
                    gameManager.CheckForWinCondition();
                }
            }
        }
    }

    public bool IsCorrect()
    {
        return currentItem != null; // Simplified: If an item is here, it must be the correct one.
    }
}

// A much simpler script for your drop zones. Its only job is to check
// the ID and notify the GameManager.
/*using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public string mountainID; // e.g., "Himalayas"
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableLabel droppedLabel = eventData.pointerDrag.GetComponent<DraggableLabel>();
            
            if (droppedLabel != null && !droppedLabel.isUnlocked && droppedLabel.mountainID == this.mountainID)
            {
                // Correct Drop!
                droppedLabel.transform.SetParent(this.transform);
                droppedLabel.transform.localPosition = Vector3.zero;
                droppedLabel.Unlock();
                
                // Notify the GameManager that a mountain has been unlocked
                gameManager.MountainUnlocked(this.mountainID);
                
                // Disable this drop zone so it can't be used again
                gameObject.GetComponent<Image>().enabled = false; // Make it invisible
                this.enabled = false; 
            }
        }
    }
}*/



