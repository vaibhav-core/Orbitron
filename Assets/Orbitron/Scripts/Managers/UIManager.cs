using UnityEngine;
using UnityEngine.UI;

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
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UIManager] Duplicate detected — destroying this instance. " +
                             "Check DontDestroyOnLoad objects for stale UIManager.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Hide panel at startup regardless of its default active state.
        // Do NOT call SetActive here — panel may not be in the hierarchy yet if
        // it is a prefab that is instantiated later. ClosePanel() handles everything.
        if (planetCreatorPanel != null)
            planetCreatorPanel.SetActive(false);
        else
            Debug.LogError("[UIManager] planetCreatorPanel is not assigned in the Inspector!");
    }

    // Update() runs regardless of Time.timeScale — Unity's Input polling is
    // not affected by timeScale, so the M key toggle works even when paused.
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (IsPaused) ClosePanel();
            else          OpenPanel();
        }
    }

    public void OpenPanel()
    {
        if (planetCreatorPanel == null)
        {
            Debug.LogError("[UIManager] OpenPanel() called but planetCreatorPanel is null. " +
                           "Assign it in the Inspector on the UIManager GameObject.");
            return;
        }

        IsPaused       = true;
        Time.timeScale = 0f;

        // Unlock the cursor so the player can interact with the UI.
        // With Input System "Both" mode, setting CursorLockMode.None is enough —
        // no need to disable the PlayerInput component.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        planetCreatorPanel.SetActive(true);

        // Ensure the panel's CanvasGroup (if any) is fully interactable.
        // A disabled CanvasGroup is the most common cause of "visible but not clickable".
        CanvasGroup cg = planetCreatorPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha          = 1f;
            cg.interactable   = true;
            cg.blocksRaycasts = true;
        }

        Debug.Log($"[UIManager] OpenPanel — timeScale={Time.timeScale}, " +
                  $"panel active={planetCreatorPanel.activeSelf}");
    }

    public void ClosePanel()
    {
        IsPaused       = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (planetCreatorPanel != null)
            planetCreatorPanel.SetActive(false);

        Debug.Log("[UIManager] ClosePanel — timeScale restored to 1.");
    }
}