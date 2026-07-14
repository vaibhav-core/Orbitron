using TMPro;
using UnityEngine;

public class EditPlanetUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlanetBrowserUI browser;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField massInput;
    [SerializeField] private TMP_InputField radiusInput;

    [SerializeField] private TMP_InputField posXInput;
    [SerializeField] private TMP_InputField posYInput;

    [SerializeField] private TMP_InputField velXInput;
    [SerializeField] private TMP_InputField velYInput;

    private BodyState currentBody;

    private const double AU_TO_KM = 149597870.7;

    private void OnEnable()
    {
        LoadSelectedPlanet();
    }

    public void LoadSelectedPlanet()
    {
        if (browser == null)
        {
            Debug.LogError("[EditPlanetUI] Browser reference missing.");
            return;
        }

        currentBody = browser.GetSelectedBody();

        if (currentBody == null)
        {
            Debug.LogWarning("[EditPlanetUI] No planet selected.");
            return;
        }

        // Mass (Solar Masses)
        massInput.text = currentBody.mass.ToString("G6");

        // Radius (km)
        radiusInput.text = (currentBody.rad * AU_TO_KM).ToString("F0");

        // Position (AU)
        posXInput.text = currentBody.pos[0].ToString("F6");
        posYInput.text = currentBody.pos[1].ToString("F6");

        // Velocity (AU/year)
        velXInput.text = currentBody.vel[0].ToString("F6");
        velYInput.text = currentBody.vel[1].ToString("F6");
    }

public void ApplyChanges()
{
    if (currentBody == null)
    {
        Debug.LogWarning("[EditPlanetUI] No planet selected.");
        return;
    }

    EditCommand command = new EditCommand();

    command.name = currentBody.name;

    // Mass (Solar Masses)
    if (double.TryParse(massInput.text, out double mass))
        command.mass = mass;

    // Radius (km -> AU)
    if (double.TryParse(radiusInput.text, out double radiusKm))
        command.rad = radiusKm / AU_TO_KM;

    // Position (AU)
    float posX = float.Parse(posXInput.text);
    float posY = float.Parse(posYInput.text);
    command.pos = new float[] { posX, posY };

    // Velocity (AU/year)
    float velX = float.Parse(velXInput.text);
    float velY = float.Parse(velYInput.text);
    command.vel = new float[] { velX, velY };

    TCPClient.Instance.SendEditCommand(command);

    Debug.Log($"Edited {command.name}");
}
}