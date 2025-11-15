using UnityEngine;

public class BirdController : MonoBehaviour
{
    [Header("Bird Settings")]
    public float hopForce = 8f;

    private Rigidbody2D rb;
    private bool isGameActive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("Bird Controller Started!");
    }

    void Update()
    {
        // Check for any touch or mouse click

        
        if (isGameActive)
        {
            bool inputDetected = false;

            // Check for mouse/touch
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                inputDetected = true;
            }

            // Check for mouse click (for testing in editor)
            if (Input.GetMouseButtonDown(0))
            {
                inputDetected = true;
            }

            // Check for space key
            if (Input.GetKeyDown(KeyCode.Space))
            {
                inputDetected = true;
            }

            if (inputDetected)
            {
                Hop();
            }
        }
    }

    void Hop()
    {
        Debug.Log("Hop called!");
        // Reset velocity and add upward force
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * hopForce, ForceMode2D.Impulse);
    }

    public void StopGame()
    {
        isGameActive = false;
    }
    public void ResetBird()
{
    // Reset bird position to starting position
    transform.position = new Vector3(-6, 0, 0); // or your preferred starting position
    
    isGameActive = true;

    // Reset velocity if using Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero;
    }
}
}