using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 5f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float pitchClamp = 89f;

    [Header("Focus")]
    [SerializeField] private float followSpeed = 5f;

    private float yaw;
    private float pitch;

    private Transform focusTarget;
    private Vector3 focusOffset;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return;

        // Follow selected body
        if (focusTarget != null)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                focusTarget.position + focusOffset,
                followSpeed * Time.deltaTime
            );
        }

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
        // Manual movement cancels focus mode
        if (Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0 ||
            Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.C))
        {
            focusTarget = null;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float vertical = 0f;

        if (Input.GetKey(KeyCode.Space))
            vertical += 1f;

        if (Input.GetKey(KeyCode.C))
            vertical -= 1f;

        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            currentSpeed *= sprintMultiplier;

        Vector3 movement =
            transform.forward * v +
            transform.right * h +
            transform.up * vertical;

        transform.position += movement * currentSpeed * Time.deltaTime;
    }

    // Called by PlanetBrowserUI
  public void FocusOn(Transform target)
{
    if (target == null)
        return;

    focusTarget = target;

    // Move camera to a nice viewing position
    Vector3 desiredOffset = new Vector3(0, 3f, -8f);

    transform.position = target.position + desiredOffset;
    focusOffset = desiredOffset;

    transform.LookAt(target);
}

    public void ClearFocus()
    {
        
        focusTarget = null;
    }
}