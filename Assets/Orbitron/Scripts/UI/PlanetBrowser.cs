using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetBrowserUI : MonoBehaviour
{
    private PlanetItemUI selectedItem;
    private string selectedPlanet;

    [SerializeField] private Transform content;
    [SerializeField] private GameObject planetItemPrefab;

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

         ui.SetName(planet);
         ui.SetBrowser(this);

         Button btn = item.GetComponentInChildren<Button>();
         btn.onClick.AddListener(ui.OnClick);
        }
    }

    public void SelectPlanet(PlanetItemUI item)
    {
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = item;
        selectedPlanet = item.PlanetName;

        selectedItem.SetSelected(true);

        Debug.Log("Selected: " + selectedPlanet);
    }
}