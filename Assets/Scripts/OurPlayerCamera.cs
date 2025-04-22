using UnityEngine;

public class OurPlayerCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody; // Typically the root or hips of the player
    public Vector3 offset = new Vector3(0f, 1.7f, 0f); // Camera offset from player (adjustable in Inspector)
    public bool rotatePlayerWithCamera = false; // Toggle to rotate player with camera or not
    
    float xRotation = 0f; // Vertical rotation (looking up/down)
    float yRotation = 0f; // Horizontal rotation (looking left/right)
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        // Make sure we have a reference to the player body
        if (playerBody == null)
        {
            Debug.LogError("Player Body reference is missing! Please assign in the Inspector.");
        }
        
        // Initialize rotation to match the player's current rotation
        if (playerBody != null)
        {
            yRotation = playerBody.eulerAngles.y;
        }
    }
    
    void LateUpdate() // Using LateUpdate for camera following to ensure player has already moved
    {
        // Check if we have a valid player reference
        if (playerBody == null) return;
        
        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // Update rotation values
        xRotation -= mouseY; // Invert Y axis for natural feeling
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limit vertical rotation
        
        yRotation += mouseX; // Add horizontal rotation
        
        // Position the camera at a fixed offset from the player
        transform.position = playerBody.position + offset;
        
        // Apply rotation to camera - first set horizontal rotation, then vertical
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        transform.Rotate(Vector3.right * xRotation);
        
        // Rotate player to match camera's horizontal rotation if enabled
        if (rotatePlayerWithCamera)
        {
            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}