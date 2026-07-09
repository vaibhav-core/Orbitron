using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 5f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float pitchClamp = 89f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return; // menu is open — UIManager owns the cursor, camera stays frozen

        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch = Mathf.Clamp(pitch - mouseY, -pitchClamp, pitchClamp);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float vertical = 0f;
        if (Input.GetKey(KeyCode.E)) vertical += 1f;
        if (Input.GetKey(KeyCode.Q)) vertical -= 1f;

        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed *= sprintMultiplier;

        Vector3 movement = transform.forward * v + transform.right * h + transform.up * vertical;
        transform.position += movement * currentSpeed * Time.deltaTime;
    }
}