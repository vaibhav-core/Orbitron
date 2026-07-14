using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject planetBrowserPanel;
    [SerializeField] private GameObject planetInfoPanel;
    [SerializeField] private GameObject spawnPanel;
    [SerializeField] private GameObject editPanel;
    [SerializeField] private GameObject collisionPanel;

    [Header("Toggle Key")]
    [SerializeField] private KeyCode toggleKey = KeyCode.M;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UIManager] Duplicate UIManager detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        HideAllPanels();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (planetBrowserPanel.activeSelf)
                ClosePlanetBrowser();
            else
                OpenPlanetBrowser();
        }
    }

    private void HideAllPanels()
    {
        if (planetBrowserPanel != null) planetBrowserPanel.SetActive(false);
        if (planetInfoPanel != null) planetInfoPanel.SetActive(false);
        if (spawnPanel != null) spawnPanel.SetActive(false);
        if (editPanel != null) editPanel.SetActive(false);
        if (collisionPanel != null) collisionPanel.SetActive(false);
    }

    // ==========================================================
    // Planet Browser
    // ==========================================================

    public void OpenPlanetBrowser()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        planetBrowserPanel.SetActive(true);
    }

    public void ClosePlanetBrowser()
    {
        HideAllPanels();

        IsPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ==========================================================
    // Planet Info
    // ==========================================================

    public void OpenPlanetInfo()
    {
        if (planetInfoPanel != null)
            planetInfoPanel.SetActive(true);
    }

    public void ClosePlanetInfo()
    {
        if (planetInfoPanel != null)
            planetInfoPanel.SetActive(false);
    }

    // ==========================================================
    // Spawn Panel
    // ==========================================================

    public void OpenSpawnPanel()
    {
        if (spawnPanel != null)
            spawnPanel.SetActive(true);
    }

    public void CloseSpawnPanel()
    {
        if (spawnPanel != null)
            spawnPanel.SetActive(false);
    }

    // ==========================================================
    // Edit Panel
    // ==========================================================

    public void OpenEditPanel()
    {
        if (editPanel != null)
            editPanel.SetActive(true);
    }

    public void CloseEditPanel()
    {
        if (editPanel != null)
            editPanel.SetActive(false);
    }

    // ==========================================================
    // Collision Panel
    // ==========================================================

    public void OpenCollisionPanel()
    {
        if (collisionPanel != null)
            collisionPanel.SetActive(true);
    }

    public void CloseCollisionPanel()
    {
        if (collisionPanel != null)
            collisionPanel.SetActive(false);
    }
}