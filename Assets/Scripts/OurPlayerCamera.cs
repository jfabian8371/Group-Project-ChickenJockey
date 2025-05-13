using UnityEngine;

public class OurPlayerCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody; // Typically the root or hips of the player

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Clamp up/down looking
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply vertical rotation to THIS camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Apply horizontal rotation to player
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
