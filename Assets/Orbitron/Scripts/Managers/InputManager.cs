using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private CameraController cameraController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void EnterGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraController != null)
            cameraController.CanControl = true;
    }

    public void EnterUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraController != null)
            cameraController.CanControl = false;
    }
}