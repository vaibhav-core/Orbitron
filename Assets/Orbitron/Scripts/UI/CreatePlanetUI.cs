using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

public class CreatePlanetUI : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Dropdown   typeDropdown;
    [SerializeField] private TMP_Dropdown   textureDropdown;

    [Header("Physical Properties")]
    [SerializeField] private TMP_InputField massInput;        // solar masses (e.g. 3e-6)
    [SerializeField] private TMP_InputField radiusInput;      // Earth radii  (e.g. 1.0 = Earth)

    [Header("Position (AU from Sun)")]
    [SerializeField] private TMP_InputField posXInput;        // AU, X axis
    [SerializeField] private TMP_InputField posYInput;        // AU, Y axis (in-plane)

    [Header("Velocity (AU/yr)")]
    [SerializeField] private TMP_InputField velXInput;        // AU/yr, X component
    [SerializeField] private TMP_InputField velYInput;        // AU/yr, Y component (orbital)

    [Header("Action")]
    [SerializeField] private Button     spawnButton;
    [SerializeField] private TMP_Text   statusText;

    // Awake() runs even when the GameObject is inactive at scene load.
    // Start() does NOT run if the GameObject is inactive — the panel starts
    // hidden, so Start() would be silently skipped and the button would have
    // no listeners. Awake() is the correct hook here.
    private void Awake()
    {
        if (spawnButton == null)
            spawnButton = GetComponentInChildren<Button>(true); // true = include inactive

        if (spawnButton != null)
        {
            spawnButton.onClick.RemoveAllListeners();
            spawnButton.onClick.AddListener(OnSpawnClicked);
        }
        else
        {
            Debug.LogError("[CreatePlanetUI] No spawn button found.");
        }
    }

    private void OnSpawnClicked()
    {
        string bName = string.IsNullOrWhiteSpace(nameInput?.text) ? "Custom_Body" : nameInput.text.Trim();

        CelestialBodyType selectedType = CelestialBodyType.Planet;
        if (typeDropdown != null) selectedType = (CelestialBodyType)typeDropdown.value;

        string textureKey = "default";
        if (textureDropdown != null && textureDropdown.options.Count > 0)
            textureKey = textureDropdown.options[textureDropdown.value].text;

        // mass: judge types solar masses directly (e.g. 3e-6 for Earth)
        TryParseDouble(massInput?.text, out double mass, 3e-6, "mass");

        // radius: judge types Earth radii (1.0 = Earth, 11.0 = Jupiter)
        // convert to AU internally before sending to Python
        TryParseDouble(radiusInput?.text, out double radiusEarthRelative, 1.0, "radius");
        double radiusAU = radiusEarthRelative * Normalizer.EarthRadiusAU;

        // position: AU in the XY simulation plane
        // UI has two fields (X, Y) matching Python's 2D coordinate system
        TryParseFloat(posXInput?.text, out float simX, 1.0f, "posX");
        TryParseFloat(posYInput?.text, out float simY, 0.0f, "posY");

        // velocity: AU/yr — judge should use v = sqrt(4π²/r) for circular orbit
        TryParseFloat(velXInput?.text, out float velX, 0.0f, "velX");
        TryParseFloat(velYInput?.text, out float velY, 6.283f, "velY");

        // pack into PlanetData using simulation-space coordinates
        // NormalizePosition converts AU → Unity units for visual placement
        PlanetData data = new PlanetData
        {
            bodyName        = bName,
            mass            = mass,
            radius          = radiusAU,
            initialPosition = Normalizer.NormalizePosition(simX, simY),
            initialVelocity = Normalizer.NormalizeVelocity(velX, velY),
            bodyType        = selectedType,
            textureKey      = textureKey
        };

        // RequestSpawn sends to Python and waits for echo-back
        // Python confirms the spawn by including it in the next state broadcast
        // PlanetManager then spawns the GameObject — no duplicate risk
        PlanetManager.Instance?.RequestSpawn(data);

        SetStatus($"Spawn request sent for '{bName}'.");
        UIManager.Instance?.ClosePanel();
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    private bool TryParseDouble(string input, out double result, double fallback, string field)
    {
        if (string.IsNullOrWhiteSpace(input)) { result = fallback; return true; }
        if (double.TryParse(input.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result)) return true;
        Debug.LogWarning($"[CreatePlanetUI] Invalid {field}: '{input}', using {fallback}");
        result = fallback;
        return false;
    }

    private bool TryParseFloat(string input, out float result, float fallback, string field)
    {
        if (string.IsNullOrWhiteSpace(input)) { result = fallback; return true; }
        if (float.TryParse(input.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result)) return true;
        Debug.LogWarning($"[CreatePlanetUI] Invalid {field}: '{input}', using {fallback}");
        result = fallback;
        return false;
    }
}