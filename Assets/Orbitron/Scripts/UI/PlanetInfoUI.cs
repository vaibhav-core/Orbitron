using TMPro;
using UnityEngine;

public class PlanetInfoUI : MonoBehaviour
{
    private const double AU_TO_KM = 149597870.7;
    private const double AU_PER_YEAR_TO_KM_PER_SEC = 4.74047;
    private const double SOLAR_MASS_TO_KG = 1.98847e30;

    [Header("Fields")]

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text parentText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private TMP_Text massText;
    [SerializeField] private TMP_Text radiusText;

    [SerializeField] private TMP_Text orbitalVelocityText;
    [SerializeField] private TMP_Text escapeVelocityText;
    [SerializeField] private TMP_Text orbitalPeriodText;
    [SerializeField] private TMP_Text energyText;

    public void ShowPlanet(BodyState body)
    {
        Debug.Log("ShowPlanet called for: " + body.name);
        nameText.text = body.name;
        parentText.text = string.IsNullOrEmpty(body.parent) ? "None" : body.parent;
        typeText.text = body.type;
        statusText.text = body.status;

        massText.text =
            $"{body.mass * SOLAR_MASS_TO_KG:0.###E0} kg";

        radiusText.text =
            $"{body.rad * AU_TO_KM:F0} km";

        orbitalVelocityText.text =
            $"{body.orbital_velocity * AU_PER_YEAR_TO_KM_PER_SEC:F2} km/s";

        escapeVelocityText.text =
            $"{body.escape_velocity * AU_PER_YEAR_TO_KM_PER_SEC:F2} km/s";

        orbitalPeriodText.text =
            $"{body.orbital_period:F2} yr";

        energyText.text =
            body.total_energy.ToString("0.###E0");
    }
}