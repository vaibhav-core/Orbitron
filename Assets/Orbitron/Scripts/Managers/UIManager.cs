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
    [SerializeField] private PlanetBrowserUI planetBrowserUI;
    [SerializeField] private PlanetInfoUI planetInfoUI;
    [SerializeField] private GameObject removePanel;

    [Header("Toggle Key")]
    [SerializeField] private KeyCode browserKey = KeyCode.M;
    [SerializeField] private KeyCode collisionKey = KeyCode.L;

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


    public void OpenRemovePanel()
{
    removePanel.SetActive(true);

    removePanel.GetComponent<RemoveConfirmationUI>().Refresh();
}

public void CloseRemovePanel()
{
    removePanel.SetActive(false);
}

 private void Update()
{
    if (Input.GetKeyDown(browserKey))
    {
        if (planetBrowserPanel.activeSelf)
            ClosePlanetBrowser();
        else
            OpenPlanetBrowser();
    }

    if (Input.GetKeyDown(collisionKey))
    {
        if (collisionPanel.activeSelf)
            CloseCollisionPanel();
        else
            OpenCollisionPanel();
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
        BodyState body = planetBrowserUI.GetSelectedBody();

        if (body == null)
            return;

        planetInfoPanel.SetActive(true);
        planetInfoUI.ShowPlanet(body);
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