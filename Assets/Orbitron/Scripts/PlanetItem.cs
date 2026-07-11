using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text planetName;
    [SerializeField] private Image background;

    public string PlanetName => planetName.text;

    public void SetName(string name)
    {
        planetName.text = name;
    }

    public void SetSelected(bool selected)
    {
        background.color = selected
            ? new Color(0.2f, 0.6f, 1f, 0.8f)
            : new Color(1f, 1f, 1f, 0.2f);
    }
}
