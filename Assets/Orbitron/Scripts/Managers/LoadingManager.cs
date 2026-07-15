using TMPro;
using UnityEngine;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TMP_Text statusText;

    private IEnumerator Start()
    {
        loadingPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        yield return Show("Loading celestial meshes...");
        yield return Show("Loading planetary textures...");
        yield return Show("Starting Python physics engine...");
        yield return Show("Connecting TCP bridge...");
        yield return Show("Initializing simulation...");
        yield return Show("Ready.");

        loadingPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private IEnumerator Show(string message)
    {
        statusText.text = message;
        yield return new WaitForSeconds(0.8f);
    }
}