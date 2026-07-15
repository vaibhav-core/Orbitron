using UnityEngine;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Loading")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private float loadingDelay = 0.8f;

    private bool gameStarted = false;
    private bool controlsOpenedFromPause = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HideAllMenus();
        StartCoroutine(StartupSequence());
    }

    private void Update()
    {
        if (!gameStarted)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel.activeSelf)
                Resume();
            else
                OpenPause();
        }
    }

    // =====================================================
    // Startup
    // =====================================================

    private IEnumerator StartupSequence()
    {
        InputManager.Instance.EnterUI();

        loadingPanel.SetActive(true);

        yield return Show("Loading UI...");
        yield return Show("Loading assets...");
        yield return Show("Ready.");

        loadingPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private IEnumerator Show(string message)
    {
        statusText.text = message;
        yield return new WaitForSeconds(loadingDelay);
    }

    private void HideAllMenus()
    {
        loadingPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);

        if (loadPanel != null)
            loadPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    // =====================================================
    // PLAY
    // =====================================================

    public void Play()
    {
        PythonLauncher.Instance.LaunchMain();

        HideAllMenus();

        gameStarted = true;

        InputManager.Instance.EnterGameplay();
    }

    // =====================================================
    // LOAD
    // =====================================================

    public void OpenLoad()
    {
        HideAllMenus();
        loadPanel.SetActive(true);
    }

    public void CloseLoad()
    {
        HideAllMenus();
        mainMenuPanel.SetActive(true);
    }

    public void LoadSolarSystem()
    {
        PythonLauncher.Instance.LaunchSolarSystem();

        HideAllMenus();

        gameStarted = true;

        InputManager.Instance.EnterGameplay();
    }

    // =====================================================
    // CONTROLS
    // =====================================================

    public void OpenControls()
    {
        controlsOpenedFromPause = pausePanel != null && pausePanel.activeSelf;

        HideAllMenus();
        controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        controlsPanel.SetActive(false);

        if (controlsOpenedFromPause)
            pausePanel.SetActive(true);
        else
            mainMenuPanel.SetActive(true);
    }

    // =====================================================
    // PAUSE
    // =====================================================

    public void OpenPause()
    {
        pausePanel.SetActive(true);

        Time.timeScale = 0f;

        InputManager.Instance.EnterUI();
    }

    public void Resume()
    {
        pausePanel.SetActive(false);

        Time.timeScale = 1f;

        InputManager.Instance.EnterGameplay();
    }

    // =====================================================
    // EXIT
    // =====================================================

    public void Exit()
    {
        PythonLauncher.Instance.CloseBackend();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}