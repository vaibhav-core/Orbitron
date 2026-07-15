using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Simulation")]
    [SerializeField] private GameObject simulationUI;

    // PLAY
    public void Play()
    {
        mainMenuPanel.SetActive(false);
        loadingPanel.SetActive(true);

        // We'll hook the actual simulation start here later.
    }

    // LOAD
    public void Load()
    {
        Debug.Log("Load not implemented yet.");
    }

    // CONTROLS
    public void OpenControls()
    {
        controlsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseControls()
    {
        controlsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // EXIT
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}