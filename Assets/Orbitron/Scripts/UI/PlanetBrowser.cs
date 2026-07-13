using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetBrowserUI : MonoBehaviour
{
    private PlanetItemUI selectedItem;
    private string selectedPlanet;

    [SerializeField] private Transform content;
    [SerializeField] private GameObject planetItemPrefab;
    [SerializeField] private PlanetInfoUI planetInfoUI;

    void Start()
    {
        string[] planets =
        {
            "Sun",
            "Mercury",
            "Venus",
            "Earth",
            "Moon",
            "Mars",
            "Jupiter"
        };

        foreach (string planet in planets)
        {
            GameObject item = Instantiate(planetItemPrefab, content);

            PlanetItemUI ui = item.GetComponent<PlanetItemUI>();

            Debug.Log($"PlanetItemUI = {ui}");

            Button btn = item.GetComponentInChildren<Button>();

            Debug.Log($"Button = {btn}");

            ui.SetName(planet);
            ui.SetBrowser(this);

            btn.onClick.AddListener(ui.OnClick);
        }
    }

    public void SelectPlanet(PlanetItemUI item)
    {
        Debug.Log("Clicked: " + item.PlanetName);
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = item;
        selectedPlanet = item.PlanetName;
        Debug.Log($"Selected: '{selectedPlanet}'");

        selectedItem.SetSelected(true);

        BodyState body = PlanetManager.Instance.GetBodyState(selectedPlanet);
        Debug.Log(body == null ? "BodyState is NULL" : "BodyState found: " + body.name);
        if (body != null)
        planetInfoUI.ShowPlanet(body);

        Debug.Log("Selected: " + selectedPlanet);
    }
}