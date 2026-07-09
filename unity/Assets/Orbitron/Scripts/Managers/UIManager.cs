using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject planetCreatorPanel;

    [Header("Toggle Key")]
    [SerializeField] private KeyCode toggleKey = KeyCode.M;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (planetCreatorPanel != null)
            planetCreatorPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (IsPaused) ClosePanel();
            else OpenPanel();
        }
    }

    public void OpenPanel()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        if (planetCreatorPanel != null)
            planetCreatorPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        if (planetCreatorPanel != null)
            planetCreatorPanel.SetActive(false);
    }
}