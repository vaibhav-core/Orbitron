using TMPro;
using UnityEngine;

public class PlanetBrowserUI : MonoBehaviour
{
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

            item.GetComponentInChildren<TMP_Text>().text = planet;
        }
    }
}