using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float sprintMultiplier = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ===== Mouse Look =====
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // ===== Movement =====
        float h = Input.GetAxis("Horizontal"); // A,D
        float v = Input.GetAxis("Vertical");   // W,S

        float vertical = 0f;

        if (Input.GetKey(KeyCode.E))
            vertical = 1f;

        if (Input.GetKey(KeyCode.Q))
            vertical = -1f;

        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            currentSpeed *= sprintMultiplier;

        Vector3 movement =
            transform.forward * v +
            transform.right * h +
            transform.up * vertical;

        transform.position += movement * currentSpeed * Time.deltaTime;
    }
}