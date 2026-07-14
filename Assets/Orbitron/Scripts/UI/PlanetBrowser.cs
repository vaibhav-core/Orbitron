using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetBrowserUI : MonoBehaviour
{
    private PlanetItemUI selectedItem;
    private string selectedPlanet;

    public string SelectedPlanet => selectedPlanet;

    [SerializeField] private Transform content;
    [SerializeField] private GameObject planetItemPrefab;
    [SerializeField] private PlanetInfoUI planetInfoUI;

    private readonly Dictionary<string, PlanetItemUI> planetItems = new();

    // Called every simulation update
    public void RefreshPlanetList(SimulationState state)
    {
        if (state == null || state.bodies == null)
            return;

        // ---------------- Add new bodies ----------------
        foreach (BodyState body in state.bodies)
        {
            if (planetItems.ContainsKey(body.name))
                continue;

            GameObject item = Instantiate(planetItemPrefab, content);

            PlanetItemUI ui = item.GetComponent<PlanetItemUI>();
            Button btn = item.GetComponentInChildren<Button>();

            ui.SetName(body.name);
            ui.SetBrowser(this);

            btn.onClick.AddListener(ui.OnClick);

            planetItems.Add(body.name, ui);
        }

        // ---------------- Remove deleted bodies ----------------
        List<string> removeList = new();

        foreach (var pair in planetItems)
        {
            bool exists = false;

            foreach (BodyState body in state.bodies)
            {
                if (body.name == pair.Key)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                removeList.Add(pair.Key);
        }

        foreach (string name in removeList)
        {
            Destroy(planetItems[name].gameObject);
            planetItems.Remove(name);

            if (selectedPlanet == name)
            {
                selectedPlanet = null;
                selectedItem = null;
            }
        }
    }

    public void SelectPlanet(PlanetItemUI item)
    {
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = item;
        selectedPlanet = item.PlanetName;

        selectedItem.SetSelected(true);

        Debug.Log($"Selected: {selectedPlanet}");
    }

    public BodyState GetSelectedBody()
    {
        if (string.IsNullOrEmpty(selectedPlanet))
            return null;

        return PlanetManager.Instance.GetBodyState(selectedPlanet);
    }
}